# Demo CI para NetCore (Jenkins+Sonarqube+Coverlet+Xunit)

Este repositorio contiene un ejemplo completo de un proyecto en NetCore y todo lo necesario para poder implementar la automatizacion del proceso de build utilizando la herramienta [Cake](https://cakebuild.net/), que luego permitira la creacion de un pipeline de CI en Jenkins.

El Proceso de build con Cake incluye:

* Compilacion de los proyectos usando `dotnet build`.
* Ejecucion de unit test usando `dotnet test` y [Coverlet](https://github.com/coverlet-coverage/coverlet)(Code Coverage)

## Proyecto NetCore y Test

El proyecto en cuestion es una aplicacion de consola la cual posee una clase llamada `BasicMaths`. Este componente nos sirve de ejemplo para la implementacion de los test de unidad realizados con [Xunit](https://xunit.net/), los cuales se utilizaran para poder tomar el porcentaje de [code coverge](https://es.wikipedia.org/wiki/Cobertura_de_c%C3%B3digo) de nuestro desarrollo.

### .editorconfig

En el raiz del repo podemos visualizar un archivo llamado `.editorconfig` el mismo ayuda a mantener estilos de codificación consistentes para múltiples desarrolladores que trabajan en el mismo proyecto en varios editores e IDE.

## Local Tools

El proyecto hace uso de herramientas locales de [NetCore](https://docs.microsoft.com/es-es/dotnet/core/tools/local-tools-how-to-use), las cuales seran instaladas en el momento que las necesitemos, lo cual las hace indispensables en una ejecucion de CI, esto significa que el proceso de CI no dependera de las herramientas instaladas en el nodo de ejecucion. 

Lo primero que debemos crear es un archivo de manifiesto  con el siguiente comando `dotnet new tool-manifest`, el cual se creara dentro de la carpeta `.config` luego de esto, cada herramienta de NetCore que instalemos sobre el repositorio sera agregada al archivo `dotnet-tools.json`.

Para esta demo se instalaron localmente 2 herramientas:

* cake.tool
* coverlet.console

Para instalar estas herramientas en un projecto nuevo se deben ejecutar los siguientes comandos luego de crear el archivo de manifiesto:

```sh

dotnet tool install cake.tool
dotnet tool install coverlet.console

```

## Build Cake

El proceso de build del proyecto se realiza con [Cake](https://cakebuild.net/) el cual es un sistema de automatizacion para tareas como la compilacion de codigo, ejecutar pruebas unitarias, etc.

Para la utilizacion de Cake, debemos tener como minimo 2 archivos en nuestro repositorio:

### build.ps1 o build.sh 

Script de arranque el cual permite la invocacion de cake. Este script ha sido [simplificado](https://andrewlock.net/simplifying-the-cake-global-tool-bootstrapper-scripts-in-netcore3-with-local-tools/) para que utilice la herramienta local de NetCore `cake.tool`. Este script es el encargado de realizar el restore de las herramientas instaladas localmente para todo nuestro proceso de build.

### buid.cake  

Este es el script de compilación real. En el mismo estan todas las tareas que necesitaremos para esta demo. Unas vez implementado el proceso de build, uno puede agregar las tareas que requiera el proyecto.

Aqui podemos visualizar las tareas y la secuencia de ejecion.

```csharp

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")

``` 

#### Build

Se realiza el build de la solucion pero primero se ejecuta el restore de los paquetes requeridos por los proyectos.

#### Test

Se ejecutan los test de unidad del proyecto utilizando la herramienta local `coverlet`. De esta ejecucion obtendremos el reporte de los test de unidad en la carpeta `test\TestResult` y el reporte de codecoverage llamado `coverage.opencover.xml` el cual se crea en el directorio raiz de la solucion.

```csharp

Task("Test")
  .Does(() => {
    var settings = new DotNetCoreToolSettings()
        {
            ArgumentCustomization = args => 
                args.Append("test\\bin\\Debug\\netcoreapp3.1\\ConsoleDemo.Test.dll")
                .Append("--target").AppendQuoted("dotnet")
                .Append("--targetargs").AppendQuoted("test test --no-build --logger trx;LogFileName=unit_tests.xml")
                .Append("--format opencover")
        };  

    DotNetCoreTool("coverlet", settings);
  });

```
Como podemos observar en el ejemplo, estamos ejecutando los unitest que se encuentra en `ConsoleDemo.Test.dll`, para un projecto nuevo, debemos modificar este valor con el nombre de la dll con nuestros test.

### Ejecucion de Prueba

Una vez que tenemos todo configurado podemos realizar una prueba de nuestro proceso de build ejecutando el script `.\build.ps1` desde la consola de powershell o `./build.sh` desde la consola de linux.

## Jenkins

Una vez que nuestro proceso de build esta funcionando localmente, podremos crear un proyecto en jenkins, para que el mismos se realice automaticamente.

### Crear Proyecto

Para crear el proyecto debemos ir a http://jenkdcsrv:8080/, una vez realizado el login:

1. Acceder a la opcion Nueva Tarea. Ingresar el nombre del proyecto y seleccionar Multibranch Pipeline luego OK.
2. Completar Display Name.
3. En Branch Sources, agregamos github como origen,
   * Credentials -> OperacionesTI/******* (Usuario para clonar de Github) 
   * Owner -> operations-innovatios o customer-experience
   * Repository -> Buscamos nuestro repositorio
   * Behaviours -> Dejamos solo "Discover branches"

4. En Build Configuration nos aseguramos de que esten estos valores, Mode -> By Jenkinsfile y Script Path -> Jenkinsfile.

5. Save

Luego de esto solo debemos verificar que la ejecucion de nuestro pipeline sea exitoso.

### Jenkinsfile

Una vez creado el projecto en Jenkins, el pipeline de ejecucion esta determinado por el contienido de este archivo.
En esta demo, utilizaremos 4 stages basico, pero al igual que el archivo build.cake, este se puede ajustar para que contenga los stages necesarios en nuestro proyecto:

1. PrepararEntorno: Realiza un clean de nuestra directorio de trabajo.
2. Clone: Se clona el repositorio.
3. Build: Se ejecuta el proceso de build con cake, igual a como lo corrimos localmente.
4. Analisis: Se realiza el analisis de codigo con sonarqube. Se aclara [aqui](#sonarqube)
5. Test: Se realiza la importacion del reporte de la ejecucion de los unit test para poder visualizar su resultado en Jenkins.

Cabe destacar que el archivo posee una seccion llamada triggers, en donde se especifica que se verificara el respositorio cada 30 min en busca de cambios. Si Jenkins detectara cambios, el proceso de build se iniciara automaticamente.

### Sonarqube

El pipeline de ejecucion de Jenkins posee un paso el cual realizara el analisis de codigo con sonarqube. Para realizar el analisis se utiliza un scanner el cual ya esta instalado en el servidor de Jenkins para que pueda ser utilizado al correr nuestro pipeline. El analisis cuenta con unos pasos que se describen mas adelante.

#### SonarBegin

Se realiza la llamada `begin` en donde se establece la Key del proyecto (en nuestro caso `dotnet-ci-demo´, si el proyecto no existe, se creara la primera vez que ejecutemos este proceso de build) y una archivo de parametros en donde podremos establecer los siguiente:

* `sonar.login`: token de autenticacion, el cual podremos obtener una vez logueados en el sitio de sonarqube en la siguiente url https://sonarqube.apps.andreani.com.ar/account/security/ 
* `sonar.cs.opencover.reportsPath`: Ruta del archivo que contendra el reporte de codecoverage en formato opencover.
* `sonar.links.scm`: El link del repositorio de github.
* `sonar.links.ci`: El link al pipeline en jenkins.
* `sonar.analysis.team`: Nombre del equipo al que pertenece este proyecto. 

#### Build

Proceso de build de la solucion.

#### SonarEnd

Se realiza la llamada `end` la cual se encarga de recuperar toda la informacion generada en el proceso de build y enviarla al servidor de sonarqube.

Si la ejecucion es exitosa podremos verificar si los reportes fueron generados correctamente y si accedemos al servidor de [sonarqube](https://sonarqube.apps.andreani.com.ar) deberias ver los valores de calidad de nuestro proyecto.

El proyecto de referencia se puede acceder desde [aqui](https://sonarqube.apps.andreani.com.ar/dashboard?id=dotnet-ci-demo)

### Demo

El proyecto actual se puede visualizar desde [aqui](http://jenkdcsrv:8080/blue/organizations/jenkins/dotnet-ci-demo/activity).

