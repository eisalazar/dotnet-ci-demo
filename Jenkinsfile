#!/usr/bin/env groovy

pipeline {
 agent {
  node {
   label 'windows'
  }
 }
triggers {
	pollSCM('H/30 * * * *')
 }	
 options {
  timestamps()
  timeout(time: 30, unit: 'MINUTES')
  disableConcurrentBuilds()
 }
 environment {
  scannerHome = tool 'SonarQube Scanner for MSBuild'
 }
 stages {
	stage('PerararEntorno') {
		steps {
			echo 'Limpiando Workspace...'
			deleteDir()
			}
	}
	stage('Clone') {
		steps {
			echo 'Git stuff...'
			checkout([$class: 'GitSCM', branches: [[name: "${env.BRANCH_NAME}"]], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: true, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], gitTool: 'git', submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'jenkinsbot', name: 'origin', url:"${env.GIT_URL}"]]]) 		
			}
	}
	 
	stage('Build') {
		steps {
			powershell ".\\build.ps1"
			}
	}
	stage('Analisis'){
		steps {
			withSonarQubeEnv('Sonarqube Interno') {
				bat "\"${scannerHome}\\\"SonarScanner.MSBuild.exe begin /k:dotnet-ci-demo /s:${env.WORKSPACE}\\SonarQube.Analysis.xml"
				powershell ".\\build.ps1"
				bat "\"${scannerHome}\\\"SonarScanner.MSBuild.exe end"
			}
		}
	}
	stage('Test'){
		steps {
			 step([$class: 'MSTestPublisher', testResultsFile:"**/unit_tests.xml", failOnError: true, keepLongStdio: true])
		}
	} 
  }
}