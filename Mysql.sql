/*
DROP DATABASE BlastCommunity
*/
/*
CREATE DATABASE BlastCommunity
CHARACTER SET utf8mb4
COLLATE utf8mb4_general_ci;
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for groupinfo
-- ----------------------------
DROP TABLE IF EXISTS `groupinfo`;
CREATE TABLE `groupinfo`  (
  `Id` int(11) NOT NULL,
  `GId` int(11) NOT NULL,
  `ItemName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Item_Remarks` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `createTime` datetime NOT NULL,
  `sort` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `creator` int(11) NOT NULL,
  `complaint` int(11) NOT NULL DEFAULT 0,
  `delete` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `GId`(`GId`) USING BTREE,
  INDEX `creator`(`creator`) USING BTREE,
  CONSTRAINT `groupinfo_ibfk_1` FOREIGN KEY (`GId`) REFERENCES `grouping` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `groupinfo_ibfk_2` FOREIGN KEY (`creator`) REFERENCES `users` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for grouping
-- ----------------------------
DROP TABLE IF EXISTS `grouping`;
CREATE TABLE `grouping`  (
  `Id` int(11) NOT NULL,
  `ItemName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `createTime` datetime NOT NULL,
  `statistics` int(11) NOT NULL DEFAULT 0,
  `sort` char(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users`  (
  `Id` int(11) NOT NULL,
  `UID` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `username` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `pwd` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `pwd_history` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `role` int(11) NOT NULL,
  `banned` int(1) NULL DEFAULT NULL,
  `banned_remarks` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `enable` int(1) NOT NULL,
  `createTime` datetime NOT NULL,
  `loginTime` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for workorder
-- ----------------------------
DROP TABLE IF EXISTS `workorder`;
CREATE TABLE `workorder`  (
  `ItemID` int(11) NOT NULL,
  `creator` int(11) NOT NULL,
  `remarks` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `violation` int(11) NOT NULL,
  `createTime` datetime NOT NULL,
  `existence` int(11) NULL DEFAULT NULL,
  INDEX `ItemID`(`ItemID`) USING BTREE,
  INDEX `creator`(`creator`) USING BTREE,
  INDEX `violation`(`violation`) USING BTREE,
  CONSTRAINT `workorder_ibfk_1` FOREIGN KEY (`ItemID`) REFERENCES `groupinfo` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `workorder_ibfk_2` FOREIGN KEY (`creator`) REFERENCES `users` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `workorder_ibfk_3` FOREIGN KEY (`violation`) REFERENCES `users` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
