-- MySQL dump 10.13  Distrib 5.6.20, for Win64 (x86_64)
--
-- Host: localhost    Database: ppgz
-- ------------------------------------------------------
-- Server version	5.6.20

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `aspnetroles`
--

DROP TABLE IF EXISTS `aspnetroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `aspnetroles` (
  `Id` varchar(128) NOT NULL,
  `Name` varchar(256) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetroles`
--

LOCK TABLES `aspnetroles` WRITE;
/*!40000 ALTER TABLE `aspnetroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserclaims`
--

DROP TABLE IF EXISTS `aspnetuserclaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `aspnetuserclaims` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` varchar(128) NOT NULL,
  `ClaimType` longtext,
  `ClaimValue` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id` (`Id`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `ApplicationUser_Claims` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserclaims`
--

LOCK TABLES `aspnetuserclaims` WRITE;
/*!40000 ALTER TABLE `aspnetuserclaims` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserclaims` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserlogins`
--

DROP TABLE IF EXISTS `aspnetuserlogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `aspnetuserlogins` (
  `LoginProvider` varchar(128) NOT NULL,
  `ProviderKey` varchar(128) NOT NULL,
  `UserId` varchar(128) NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`,`UserId`),
  KEY `ApplicationUser_Logins` (`UserId`),
  CONSTRAINT `ApplicationUser_Logins` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserlogins`
--

LOCK TABLES `aspnetuserlogins` WRITE;
/*!40000 ALTER TABLE `aspnetuserlogins` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserlogins` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetuserroles`
--

DROP TABLE IF EXISTS `aspnetuserroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `aspnetuserroles` (
  `UserId` varchar(128) NOT NULL,
  `RoleId` varchar(128) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IdentityRole_Users` (`RoleId`),
  CONSTRAINT `ApplicationUser_Roles` FOREIGN KEY (`UserId`) REFERENCES `aspnetusers` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `IdentityRole_Users` FOREIGN KEY (`RoleId`) REFERENCES `aspnetroles` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetuserroles`
--

LOCK TABLES `aspnetuserroles` WRITE;
/*!40000 ALTER TABLE `aspnetuserroles` DISABLE KEYS */;
/*!40000 ALTER TABLE `aspnetuserroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `aspnetusers`
--

DROP TABLE IF EXISTS `aspnetusers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `aspnetusers` (
  `Id` varchar(128) NOT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext,
  `SecurityStamp` longtext,
  `PhoneNumber` longtext,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEndDateUtc` datetime DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int(11) NOT NULL,
  `UserName` varchar(256) NOT NULL,
  `activo` tinyint(1) DEFAULT NULL,
  `aspnetuserscol` varchar(45) DEFAULT NULL,
  `perfil_id` int(11) DEFAULT NULL,
  `tipo_usuario_id` int(11) DEFAULT NULL,
  `nombre` varchar(100) DEFAULT NULL,
  `apellido` varchar(100) DEFAULT NULL,
  `cargo` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `aspnetusers`
--

LOCK TABLES `aspnetusers` WRITE;
/*!40000 ALTER TABLE `aspnetusers` DISABLE KEYS */;
INSERT INTO `aspnetusers` VALUES ('5c329efe-f3c9-4e97-98b1-2e5ff043e323','umpmercaderia@umpmercaderia.com',0,'AMG34S6Lc0izOXxZyCtb3sMYJuptHTUPUymGnQrXUjtcr2ADeIOlwCBsB54J9RvkPw==','a58772a2-532d-4629-94c1-2ac6ca6a23e6','1231321',0,0,NULL,0,0,'umpmercaderia',NULL,NULL,NULL,1,'proveedor de pruebas','umpmercaderia','umpmercaderia'),('d94bfacc-f1eb-4b1e-aec3-f0a097a0548f',NULL,0,'ANt50PjNvfVSKIbezih/vhy6bc4tlruOHtJU0nOtEAHQWl+PFkoBKthqzi1YxZ+siw==','e8ccccfd-6a5f-4ea7-b475-9628c559b18f',NULL,0,0,NULL,0,0,'superadmin',NULL,NULL,NULL,NULL,NULL,NULL,NULL),('dc8c6c8c-db7d-476b-8216-c3d6b242c1ca','umservicio@umservicio.com',0,'ALi526dAzC3ygfKRJraQDXAIjl/Tovtbzve8bliiQWLDFafV8Y0SRCE3kZm/X19ZsQ==','a8fbcd8a-8363-47de-8ca1-a486be15f5b3','1231231231',0,0,NULL,0,0,'umservicio',NULL,NULL,NULL,2,'umservicio','umservicio','umservicio');
/*!40000 ALTER TABLE `aspnetusers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cuentas`
--

DROP TABLE IF EXISTS `cuentas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `cuentas` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `codigo_proveedor` varchar(50) NOT NULL,
  `nombre_proveedor` varchar(200) NOT NULL,
  `fecha_registro` datetime NOT NULL,
  `tipo_proveedor_id` int(11) NOT NULL,
  `activo` tinyint(1) DEFAULT NULL,
  `reponsable_usuario_id` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_responsable_idx` (`reponsable_usuario_id`),
  CONSTRAINT `fk_responsable` FOREIGN KEY (`reponsable_usuario_id`) REFERENCES `aspnetusers` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cuentas`
--

LOCK TABLES `cuentas` WRITE;
/*!40000 ALTER TABLE `cuentas` DISABLE KEYS */;
INSERT INTO `cuentas` VALUES (1,'a377393a-b6b9-4381-812f-751e59ec8f3d','proveedor de pruebas','0001-01-01 00:00:00',1,1,'5c329efe-f3c9-4e97-98b1-2e5ff043e323'),(2,'1d68a669-cd22-4646-98e7-0861ef3fb116','umservicio','0001-01-01 00:00:00',2,1,'dc8c6c8c-db7d-476b-8216-c3d6b242c1ca');
/*!40000 ALTER TABLE `cuentas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mensajes`
--

DROP TABLE IF EXISTS `mensajes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mensajes` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `titulo` varchar(100) NOT NULL,
  `contenido` longtext,
  `archivo` varchar(100) DEFAULT NULL,
  `fecha_publicacion` datetime DEFAULT NULL,
  `fecha_caducidad` datetime DEFAULT NULL,
  `enviado_a` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mensajes`
--

LOCK TABLES `mensajes` WRITE;
/*!40000 ALTER TABLE `mensajes` DISABLE KEYS */;
INSERT INTO `mensajes` VALUES (1,'PRUEBA DE MENSAJE MERCADERÍA','TEXTO DEL MENSAJE',NULL,'2017-02-22 00:00:00','2017-02-24 00:00:00','MERCADERIA'),(2,'PRUEBA DE MENSAJE SERVICIO','CONTENIDO DEL MENSAJE',NULL,'2017-02-22 00:00:00','2017-02-26 00:00:00','SERVICIO'),(3,'PRUEBA DE MENSAJE TODOS','CONTENIDO DEL MENSAJE',NULL,'2017-02-22 00:00:00','2017-02-26 00:00:00','TODOS');
/*!40000 ALTER TABLE `mensajes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_proveedor`
--

DROP TABLE IF EXISTS `tipos_proveedor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tipos_proveedor` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `codigo` varchar(45) NOT NULL,
  `nombre` varchar(45) NOT NULL,
  `descripcion` varchar(200) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `codigo_UNIQUE` (`codigo`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_proveedor`
--

LOCK TABLES `tipos_proveedor` WRITE;
/*!40000 ALTER TABLE `tipos_proveedor` DISABLE KEYS */;
INSERT INTO `tipos_proveedor` VALUES (1,'MERCADERIA','Mercaderia','Mercaderia'),(2,'SERVICIO','Servicio','Servicio');
/*!40000 ALTER TABLE `tipos_proveedor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_usuario`
--

DROP TABLE IF EXISTS `tipos_usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tipos_usuario` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `codigo` varchar(45) NOT NULL,
  `nombre` varchar(200) NOT NULL,
  `descripcion` varchar(2000) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_usuario`
--

LOCK TABLES `tipos_usuario` WRITE;
/*!40000 ALTER TABLE `tipos_usuario` DISABLE KEYS */;
INSERT INTO `tipos_usuario` VALUES (1,'NAZAN','Usuario Nazan','Usuario Nazan'),(2,'PROVEEDOR','Proveedor ','Proveedor');
/*!40000 ALTER TABLE `tipos_usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario_mensajes`
--

DROP TABLE IF EXISTS `usuario_mensajes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `usuario_mensajes` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `mensaje_id` int(11) NOT NULL,
  `usuario_id` varchar(128) NOT NULL,
  `fecha_visualizacion` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_mensaje_idx` (`mensaje_id`),
  KEY `fk_usuario_idx` (`usuario_id`),
  CONSTRAINT `fk_mensaje` FOREIGN KEY (`mensaje_id`) REFERENCES `mensajes` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `fk_usuario` FOREIGN KEY (`usuario_id`) REFERENCES `aspnetusers` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario_mensajes`
--

LOCK TABLES `usuario_mensajes` WRITE;
/*!40000 ALTER TABLE `usuario_mensajes` DISABLE KEYS */;
INSERT INTO `usuario_mensajes` VALUES (1,1,'dc8c6c8c-db7d-476b-8216-c3d6b242c1ca',NULL),(2,2,'dc8c6c8c-db7d-476b-8216-c3d6b242c1ca',NULL);
/*!40000 ALTER TABLE `usuario_mensajes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios_cuentas_xref`
--

DROP TABLE IF EXISTS `usuarios_cuentas_xref`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `usuarios_cuentas_xref` (
  `usuario_id` varchar(128) NOT NULL,
  `cuenta_id` int(11) NOT NULL,
  PRIMARY KEY (`usuario_id`,`cuenta_id`),
  KEY `fk_cuentas_idx` (`cuenta_id`),
  CONSTRAINT `fk_cuentas` FOREIGN KEY (`cuenta_id`) REFERENCES `cuentas` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  CONSTRAINT `fk_usuarios` FOREIGN KEY (`usuario_id`) REFERENCES `aspnetusers` (`Id`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios_cuentas_xref`
--

LOCK TABLES `usuarios_cuentas_xref` WRITE;
/*!40000 ALTER TABLE `usuarios_cuentas_xref` DISABLE KEYS */;
INSERT INTO `usuarios_cuentas_xref` VALUES ('5c329efe-f3c9-4e97-98b1-2e5ff043e323',1),('dc8c6c8c-db7d-476b-8216-c3d6b242c1ca',2);
/*!40000 ALTER TABLE `usuarios_cuentas_xref` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2017-02-22 11:34:21
