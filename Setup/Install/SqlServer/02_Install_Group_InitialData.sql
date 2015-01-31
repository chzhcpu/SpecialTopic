-----应用
DELETE FROM [dbo].[tn_Applications] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_Applications] ([ApplicationId], [ApplicationKey], [Description], [IsEnabled], [IsLocked], [DisplayOrder]) VALUES (9002, N'Group', N'群组应用', 1, 0, 9002)

-----应用在呈现区域相关设置
DELETE FROM [dbo].[tn_ApplicationInPresentAreaSettings] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (9002, N'Channel', 0, 1, 1)
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (9002, N'UserSpace', 0, 1, 0)

----快捷操作
DELETE FROM [dbo].[tn_ApplicationManagementOperations] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10900201, 9002, 0, N'Channel', 1, N'创建群组', N'', N'', N'Channel_Group_Create', NULL, N'UserRelation', NULL, N'_blank', 10900201, 0, 1, 1)
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20900201, 9002, 0, N'ControlPanel', 1, N'群组类别管理', N'', N'', N'ControlPanel_Content_ManageSiteCategories', N'tenantTypeId', NULL, NULL, N'_blank', 20900201, 1, 0, 1)

-----导航
DELETE FROM [dbo].[tn_InitialNavigations] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10900201, 0, 0, N'Channel', 9002, 0, N'群组', N'', N'', N'Channel_Group_Home', NULL, N'Group', NULL, N'_self', 10900201, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10900202, 10900201, 1, N'Channel', 9002, 0, N'群组首页', N' ', N' ', N'Channel_Group_Home', NULL, NULL, NULL, N'_self', 10900202, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10900203, 10900201, 1, N'Channel', 9002, 0, N'我的群组', N'', N'', N'Channel_Group_UserGroups', N'spaceKey', NULL, NULL, N'_self', 10900203, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10900204, 10900201, 1, N'Channel', 9002, 0, N'发现群组', N'', N'', N'Channel_Group_FindGroup', NULL, NULL, NULL, N'_self', 10900204, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11900201, 0, 0, N'UserSpace', 9002, 0, N'群组', N' ', N' ', N'Channel_Group_UserGroups', N'spaceKey', N'Group', NULL, N'_self', 11900201, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (13900180, 0, 0, N'GroupSpace', 0, 1, N'成员', N' ', N' ', N'GroupSpace_Member', NULL, NULL, NULL, N'_self', 13101280, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (13900190, 0, 0, N'GroupSpace', 0, 1, N'管理', N' ', N' ', N'Group_Bar_ManageThreads', NULL, NULL, NULL, N'_self', 13900290, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20900201, 20000011, 2, N'ControlPanel', 9002, 0, N'群组', N'', N'', N'ControlPanel_Group_Home', NULL, NULL, NULL, N'_self', 20900201, 0, 0, 1)

-----默认安装记录
DELETE FROM [dbo].[tn_ApplicationInPresentAreaInstallations] WHERE [ApplicationId] = 9002 and OwnerId = 0
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 9002, 'Channel')

-----动态
DELETE FROM  [dbo].[tn_ActivityItems] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateGroup', 9002, N'创建群组', 0, N'', 0, 1, 1)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'JoinGroup', 9002, N'加入群组', 0, N'', 0, 1, 0)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateGroupMember', 9002, N'新成员加入', 0, N'', 1, 0, 0)

-----用户角色
DELETE FROM [dbo].[tn_Roles] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_Roles] ([RoleName], [FriendlyRoleName], [IsBuiltIn], [ConnectToUser], [ApplicationId], [IsPublic], [Description], [IsEnabled], [RoleImage]) VALUES (N'GroupAdministrator', N'群组管理员', 1, 1, 9002, 1, N'管理群组应用下的内容', 1, N'')

-----审核
DELETE FROM [dbo].[tn_AuditItems] WHERE [ApplicationId] = 9002
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Group', 9002, N'创建群组', 11, N'')

-----审核规则
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'RegisteredUsers',N'Group',2 ,0)
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'ModeratedUser',N'Group',2 ,0)

-----积分
DELETE FROM [dbo].[tn_PointItems] WHERE [ApplicationId]=9002
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Group_CreateGroup', 9002, N'创建群组', 101, 100, 50, 50, 0, 0, 0, N'',1)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Group_DeleteGroup', 9002, N'删除群组', 102, -100, -50, -50, 0, 0, 0, N'',0)

-----租户类型
DELETE FROM [dbo].[tn_TenantTypes] WHERE [ApplicationId]=9002
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'900200', 9002, N'群组', N'Spacebuilder.Group.GroupEntity,Spacebuilder.Group')

-----租户使相关服务
DELETE FROM [dbo].[tn_TenantTypesInServices] WHERE [TenantTypeId]='900200'
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'900200', N'Count')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'900200', N'SiteCategory')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'900200', N'Tag')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'900200', N'Recommend')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'900200', N'Visit')

-----自运行任务
DELETE FROM [dbo].[tn_TaskDetails] WHERE [ClassType]=N'Spacebuilder.Group.CalculateGrowthValuesTask,Spacebuilder.Group'
INSERT [dbo].[tn_TaskDetails] ([Name], [TaskRule], [ClassType], [Enabled], [RunAtRestart], [IsRunning], [LastStart], [LastEnd], [LastIsSuccess], [NextStart], [StartDate], [EndDate], [RunAtServer]) VALUES (N'更新群组的成长值', N'0 0 0/12 * * ?', N'Spacebuilder.Group.CalculateGrowthValuesTask,Spacebuilder.Group', 1, 0, 0, N'', N'', 1, N'', N'', NULL, 0)

-----推荐
DELETE FROM [dbo].[tn_RecommendItemTypes] WHERE [TypeId] IN ('00001111','90020001')
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'00001111', N'000011', N'推荐群主', N'推荐群主', 0, N'')
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'90020001', N'900200', N'推荐群组', N'推荐群组', 0, N'')

-----类别
DELETE FROM [dbo].[tn_Categories] WHERE [TenantTypeId] = '900200'
SET IDENTITY_INSERT [tn_Categories] ON
INSERT [tn_Categories] ([CategoryId], [ParentId], [OwnerId], [TenantTypeId], [CategoryName], [Description], [DisplayOrder], [Depth], [ChildCount], [ItemCount], [PrivacyStatus], [AuditStatus], [FeaturedItemId], [LastModified], [DateCreated], [PropertyNames], [PropertyValues]) VALUES (28, 0, 0, N'900200', N'默认类别', N'', 28, 0, 0, 0, 2, 40, 0, CAST(0x0000A187003CE170 AS DateTime), CAST(0x0000A187003376B7 AS DateTime), NULL, NULL)
SET IDENTITY_INSERT [tn_Categories] OFF

-----广告位
DELETE FROM [dbo].[tn_AdvertisingPosition] WHERE [PositionId] like '109002%' or [PositionId] like '139002%'
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10900200001', N'Channel', N'群组频道首页中上部广告位(550x190)', N'AdvertisingPosition\00001\09002\00001\10900200001.jpg', 550, 190, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10900200002', N'Channel', N'群组频道首页左中部广告位(190x70)', N'AdvertisingPosition\00001\09002\00002\10900200002.jpg', 190, 70, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'13900200003', N'GroupSpace', N'群组详细显示页中部广告位(710x100)', N'AdvertisingPosition\00001\39002\00003\13900200003.jpg', 710, 100, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'13900200004', N'GroupSpace', N'群组讨论详细显示页中部广告位(710x70)', N'AdvertisingPosition\00001\39002\00004\13900200004.jpg', 710, 70, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'13900200005', N'GroupSpace', N'群组讨论详细显示页右下部广告位(230x260)', N'AdvertisingPosition\00001\39002\00005\13900200005.jpg', 230, 260, 1)