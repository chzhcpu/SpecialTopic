﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using Tunynet.Utilities;
using System.Web.Routing;
using DevTrends.MvcDonutCaching;

namespace SpecialTopic.Topic.Controllers
{
    [Themed(PresentAreaKeysOfExtension.TopicSpace, IsApplication = true)]
    [AnonymousBrowseCheck]
    [TitleFilter(IsAppendSiteName = true)]
    [TopicSpaceAuthorize]
    public class TopicSpaceController : Controller
    {
        public TopicService groupService { get; set; }
        public IPageResourceManager pageResourceManager { get; set; }
        public IUserService userService { get; set; }
        public FollowService followService { get; set; }
        public ActivityService activityService { get; set; }
        public ApplicationService applicationService { get; set; }
        public PrivacyService privacyService { get; set; }
        public Authorizer authorizer { get; set; }
        private VisitService visitService = new VisitService(TenantTypeIds.Instance().Topic());
        private SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().Topic());

        /// <summary>
        /// 专题标签云
        /// </summary>_CommentList
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">显示的标签数量</param>
        /// <param name="ownerId">拥有者ID</param>
        /// <param name="showInNewPage">新页显示</param>
        /// <returns></returns>
        public ActionResult _TagCloud(string tenantTypeId = "", int topNumber = 20, long ownerId = 0, bool showInNewPage = false)
        {
            TagService tagService = new TagService(tenantTypeId);
            Dictionary<TagInOwner, int> groupTags = tagService.GetOwnerTopTags(topNumber, ownerId);

            ViewData["ownerId"] = ownerId;
            ViewData["showInNewPage"] = showInNewPage;
            return View(groupTags);
        }

        /// <summary>
        /// 最近访客控件
        /// </summary>
        public ActionResult _LastTopicVisitors(string spaceKey, int topNumber = 12)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IEnumerable<Visit> visits = visitService.GetTopVisits(groupId, topNumber);
            ViewData["groupId"] = groupId;
            return View(visits);
        }

        /// <summary>
        /// 专题首页动态列表
        /// </summary>
        [HttpGet]
        public ActionResult _ListActivities(string spaceKey, int? pageIndex, int? applicationId, MediaType? mediaType, bool? isOriginal, long? userId)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            PagingDataSet<Activity> activities = activityService.GetOwnerActivities(ActivityOwnerTypes.Instance().Topic(), groupId, applicationId, mediaType, isOriginal, null, pageIndex ?? 1, userId);
            if (activities.FirstOrDefault() != null)
            {
                ViewData["lastActivityId"] = activities.FirstOrDefault().ActivityId;
            }
            ViewData["pageIndex"] = pageIndex;
            ViewData["applicationId"] = applicationId;
            ViewData["mediaType"] = mediaType;
            ViewData["isOriginal"] = isOriginal;
            ViewData["userId"] = userId;
            return View(activities);
        }

        /// <summary>
        /// 获取以后进入用户时间线的动态
        /// </summary>
        public ActionResult _GetNewerActivities(string spaceKey, int? applicationId, long? lastActivityId = 0)
        {
            if (UserContext.CurrentUser == null)
                return new EmptyResult();
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IEnumerable<Activity> newActivities = activityService.GetNewerActivities(groupId, lastActivityId.Value, applicationId, ActivityOwnerTypes.Instance().Topic(), UserContext.CurrentUser.UserId);

            if (newActivities != null && newActivities.Count() > 0)
            {
                ViewData["lastActivityId"] = newActivities.FirstOrDefault().ActivityId;
            }
            return View(newActivities);
        }

        /// <summary>
        ///  查询自lastActivityId以后又有多少动态进入用户的时间线
        /// </summary>
        [HttpPost]
        public JsonResult GetNewerTopicActivityCount(string spaceKey, long lastActivityId, int? applicationId)
        {
            if (UserContext.CurrentUser == null)
                return Json(new { });
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            string name;
            int count = activityService.GetNewerCount(groupId, lastActivityId, applicationId, out name, ActivityOwnerTypes.Instance().Topic(), UserContext.CurrentUser.UserId);
            if (count == 0)
            {
                return Json(new { lastActivityId = lastActivityId, hasNew = false });
            }
            else
            {
                string showName;
                if (count == 1)
                    showName = name + "更新了动态，点击查看";
                else
                    showName = name + "等多位群友更新了动态，点击查看";
                return Json(new { hasNew = true, showName = showName });
            }
        }

        /// <summary>
        /// 删除专题动态
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeleteTopicActivity(string spaceKey, long activityId)
        {
            var activity = activityService.Get(activityId);
            if (!authorizer.Topic_DeleteTopicActivity(activity))
                return Json(new StatusMessageData(StatusMessageType.Error, "没有删除专题动态的权限"));
            activityService.DeleteActivity(activityId);
            return Json(new StatusMessageData(StatusMessageType.Success, "删除专题动态成功！"));
        }


        /// <summary>
        /// 删除该条访客记录
        /// </summary>
        [HttpPost]
        public ActionResult DeleteTopicVisitor(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));

            //如何做

            //已修复
            long userId = Request.Form.Get<long>("userId", 0);
            long id = Request.Form.Get<long>("id", 0);
            if (authorizer.Topic_DeleteVisitor(group.TopicId, userId))
            {
                visitService.Delete(id);
                return RedirectToAction("_LastTopicVisitors");
            }
            else
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "没有删除该条访客记录的权限！"));
            }
        }


        /// <summary>
        /// 群友还喜欢去
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _TopicMemberAlsoJoinedTopics(string spaceKey, int topNumber = 10)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IEnumerable<TopicEntity> groups = groupService.TopicMemberAlsoJoinedTopics(groupId, topNumber);
            return View(groups);
        }

        /// <summary>
        /// 编辑专题公告页
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _EditAnnouncement(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            ViewData["announcement"] = group.Announcement;
            return View();
        }
        /// <summary>
        /// 在线的专题成员
        /// </summary>
        /// <param name="spaceKey">专题</param>
        /// <param name="topNumber">前多少条</param>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _OnlineTopicMembers(string spaceKey, int topNumber = 12)
        {
            TopicEntity group = groupService.Get(spaceKey);
            ViewData["User"] = group.User;
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IEnumerable<TopicMember> groupMembers = groupService.GetOnlineTopicMembers(groupId);
            if (group.User.IsOnline)
            {
                return View(groupMembers.Take(topNumber - 1));
            }
            else
            {
                return View(groupMembers.Take(topNumber));
            }
        }
        /// <summary>
        /// 专题资料
        /// </summary>
        /// <param name="spaceKey">专题标识</param>
        /// <returns></returns>
        public ActionResult _TopicProfile(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            return View(group);
        }

        /// <summary>
        /// 编辑专题公告
        /// </summary>
        /// <param name="spaceKey">专题标识</param>
        /// <param name="announcement">公告</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _EditAnnouncement(string spaceKey, string announcement)
        {
            string errorMessage = null;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
            }

            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return Json(new { });
            if (!authorizer.Topic_Manage(group))
                return Json(new StatusMessageData(StatusMessageType.Error, "没有更新公告的权限"));

            groupService.UpdateAnnouncement(group.TopicId, announcement);
            return Json(new { shortAnnouncement = StringUtility.Trim(announcement, 100), longAnnouncement = announcement });
        }


        /// <summary>
        /// 邀请好友
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _Invite(string spaceKey)
        {
            return View();
        }



        /// <summary>
        /// 邀请好友
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _Invite(string spaceKey, string userIds, string remark)
        {
            StatusMessageData message = null;
            string unInviteFriendNames = string.Empty;
            TopicEntity group = groupService.Get(spaceKey);


            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));

            //在显示时做了判断
            //已修改
            IUser currentUser = UserContext.CurrentUser;

            List<long> couldBeInvetedUserIds = new List<long>();
            //被邀请人的隐私设置
            IEnumerable<long> inviteUserIds = Request.Form.Gets<long>("userIds", null);
            int count = 0;
            foreach (long inviteUserId in inviteUserIds)
            {

                if (!privacyService.Validate(inviteUserId, currentUser != null ? currentUser.UserId : 0, PrivacyItemKeys.Instance().Invitation()))
                {
                    User user = userService.GetFullUser(inviteUserId);
                    unInviteFriendNames += user.DisplayName + ",";

                }
                else
                {
                    count++;
                    couldBeInvetedUserIds.Add(inviteUserId);
                }
            }



            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));

            if (!authorizer.Topic_Invite(group))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有邀请好友的权限！",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            if (!string.IsNullOrEmpty(userIds))
            {

                //已修改

                IEnumerable<long> ids = Request.Form.Gets<long>("userIds", null);
                if (ids != null && ids.Count() > 0)
                {
                    groupService.SendInvitations(group, currentUser, remark, couldBeInvetedUserIds);
                    if (count < ids.Count())
                    {
                        message = new StatusMessageData(StatusMessageType.Hint, "共有" + count + "个好友邀请成功，" + unInviteFriendNames.Substring(0, unInviteFriendNames.Count() - 1) + "不能被邀请！");
                    }
                    else
                    {
                        message = new StatusMessageData(StatusMessageType.Success, "邀请好友成功！");
                    }
                }
                else
                {
                    message = new StatusMessageData(StatusMessageType.Hint, "您尚未选择好友！");
                }
            }
            return Json(message);
        }

        /// <summary>
        /// 成员列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Members(string spaceKey, int pageIndex = 1)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            pageResourceManager.InsertTitlePart(group.TopicName);
            pageResourceManager.InsertTitlePart("管理成员列表页");
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            PagingDataSet<TopicMember> groupMembers = groupService.GetTopicMembers(groupId, false, pageSize: 60, pageIndex: pageIndex);




            ViewData["Topic"] = group;

            return View(groupMembers);
        }

        /// <summary>
        /// 我关注的专题成员
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult MyFollowedUsers(string spaceKey, int pageIndex = 1)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            var currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Redirect(SiteUrls.Instance().Login(true));

            pageResourceManager.InsertTitlePart(group.TopicName);
            pageResourceManager.InsertTitlePart("管理成员列表页");

            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IEnumerable<TopicMember> groupMember = groupService.GetTopicMembersAlsoIsMyFollowedUser(groupId, currentUser.UserId);
            PagingDataSet<TopicMember> groupMembers = new PagingDataSet<TopicMember>(groupMember);

            if (currentUser.IsFollowed(group.User.UserId))
            {
                ViewData["groupOwner"] = group.User;
            }




            return View(groupMembers);
        }

        /// <summary>
        /// 删除管理员
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ActionResult DeleteManager(string spaceKey, long userId)
        {
            TopicEntity group = groupService.Get(spaceKey);


            if (!authorizer.Topic_DeleteMember(group, userId))
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有删除管理员的权限"));

            groupService.DeleteTopicMember(group.TopicId, userId);
            return Json(new StatusMessageData(StatusMessageType.Success, "删除成功"));
        }


        /// <summary>
        /// 最新加入
        /// </summary>
        /// <param name="spaceKey">专题标识</param>
        /// <param name="topNumber">前几条数据</param>
        /// <returns></returns>
        public ActionResult _ListMembers(string spaceKey, int topNumber)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return new EmptyResult();
            PagingDataSet<TopicMember> groupMembers = groupService.GetTopicMembers(groupId, false, SortBy_TopicMember.DateCreated_Desc);
            IEnumerable<TopicMember> members = groupMembers.Take(topNumber);

            return View(members);
        }

        /// <summary>
        /// 专题空间导航
        /// </summary>
        /// <param name="spaceKey">专题空间标识</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _TopicMenu(string spaceKey)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetTopicId(spaceKey);
            TopicEntity group = groupService.Get(groupId);
            if (group == null)
                return Content(string.Empty);

            int currentNavigationId = RouteData.Values.Get<int>("CurrentNavigationId", 0);
            IEnumerable<Navigation> navigations = new List<Navigation>();

            NavigationService navigationService = new NavigationService();
            Navigation navigation = navigationService.GetNavigation(PresentAreaKeysOfBuiltIn.GroupSpace, currentNavigationId, group.TopicId);

            if (navigation != null && navigation.Children.Count() > 0)
            {
                navigations = navigation.Children;
            }
            else
            {
                navigations = navigationService.GetRootNavigations(PresentAreaKeysOfBuiltIn.GroupSpace, group.TopicId);
            }

            return View(navigations);
        }

        /// <summary>
        /// 公告
        /// </summary>
        /// <param name="spaceKey">专题标识</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _Announcement(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            return View(group);
        }

        /// <summary>
        /// 专题首页动态
        /// </summary>
        /// <param name="spaceKey">专题标识</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _TopicActivities(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();

            IEnumerable<ApplicationBase> applications = applicationService.GetInstalledApplicationsOfOwner(PresentAreaKeysOfBuiltIn.GroupSpace, group.TopicId);
            ViewData["applications"] = applications;
            return View(group);
        }
    }

    public enum TopicMenu
    {
        /// <summary>
        /// 主页
        /// </summary>
        Home,

        /// <summary>
        /// 管理成员
        /// </summary>
        ManageMember,

        /// <summary>
        /// 专题设置
        /// </summary>
        TopicSettings

    }
}
