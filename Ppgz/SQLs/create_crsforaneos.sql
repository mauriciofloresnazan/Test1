CREATE TABLE `crsforaneos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Proveedor` varchar(20) DEFAULT NULL,
  `ArchivoCR` varchar(1000) DEFAULT NULL,
  `Fecha` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8;
