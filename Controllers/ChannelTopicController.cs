//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tunynet.Common;
using Tunynet.UI;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Search;
using Spacebuilder.Search;

using Tunynet.Common.Configuration;
using System.Text.RegularExpressions;
using Tunynet.Utilities;
using DevTrends.MvcDonutCaching;

namespace SpecialTopic.Topic.Controllers 
{
    /// <summary>
    /// 频道专题控制器
    /// </summary>
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    [TitleFilter(IsAppendSiteName = true)]
    public class ChannelTopicController : Controller
    {
        public ActivityService activityService { get; set; }
        public IPageResourceManager pageResourceManager { get; set; }
        public CategoryService categoryService { get; set; }
        public TopicService topicService { get; set; }
        public Authorizer authorizer { get; set; }
        public IdentificationService identificationService { get; set; }
        public RecommendService recommendService { get; set; }
        public UserService userService { get; set; }
        public AreaService areaService { get; set; }
        public FollowService followService { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().Topic());

        /// <summary>
        /// 频道专题
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Home()
        {
            pageResourceManager.InsertTitlePart("专题首页");
            return View();
        }

        /// <summary>
        /// 专题顶部的局部页面
        /// </summary>
        /// <returns></returns>
        public ActionResult _TopicSubmenu()
        {
            return View();
        }

        /// <summary>
        /// 验证专题Key的方法
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ValidateTopicKey(string groupKey, long groupId)
        {
            bool result = false;
            if (groupId > 0)
            {
                result = true;
            }
            else
            {
                TopicEntity group = topicService.Get(groupKey);
                if (group != null)
                {
                    return Json("此专题Key已存在", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    result = Regex.IsMatch(groupKey, @"^[A-Za-z0-9_\-\u4e00-\u9fa5]+$", RegexOptions.IgnoreCase);
                    if (!result)
                    {
                        return Json("只能输入字母数字汉字或-号", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建专题
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            pageResourceManager.InsertTitlePart("创建专题");
            string errorMessage = null;
            if (!authorizer.Topic_Create(out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = errorMessage,
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            TopicEditModel group = new TopicEditModel();
            return View(group);
        }


        //已修改
        /// <summary>
        /// 创建专题
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(TopicEditModel groupEditModel)
        {
            string errorMessage = null;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = "创建失败",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            System.IO.Stream stream = null;
            HttpPostedFileBase groupLogo = Request.Files["GroupLogo"];


            //已修改
            IUser user = UserContext.CurrentUser;
            if (user == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));

            if (!authorizer.Topic_Create(out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = errorMessage,
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            if (groupLogo != null && !string.IsNullOrEmpty(groupLogo.FileName))
            {
                TenantLogoSettings tenantLogoSettings = TenantLogoSettings.GetRegisteredSettings(TenantTypeIds.Instance().Topic());
                if (!tenantLogoSettings.ValidateFileLength(groupLogo.ContentLength))
                {
                    ViewData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Error, string.Format("文件大小不允许超过{0}", Formatter.FormatFriendlyFileSize(tenantLogoSettings.MaxLogoLength * 1024)));
                    return View(groupEditModel);
                }

                LogoSettings logoSettings = DIContainer.Resolve<ISettingsManager<LogoSettings>>().Get();
                if (!logoSettings.ValidateFileExtensions(groupLogo.FileName))
                {
                    ViewData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Error, "不支持的文件类型，仅支持" + logoSettings.AllowedFileExtensions);
                    return View(groupEditModel);
                }
                stream = groupLogo.InputStream;
                groupEditModel.Logo = groupLogo.FileName;
            }
            TopicEntity group = groupEditModel.AsTopicEntity();

            bool result = topicService.Create(user.UserId, group);

            if (stream != null)
            {
                topicService.UploadLogo(group.TopicId, stream);
            }
            //设置分类
            if (groupEditModel.CategoryId > 0)
            {
                categoryService.AddItemsToCategory(new List<long>() { group.TopicId }, groupEditModel.CategoryId);
            }
            //设置标签
            string relatedTags = Request.Form.Get<string>("RelatedTags");
            if (!string.IsNullOrEmpty(relatedTags))
            {
                tagService.AddTagsToItem(relatedTags, group.TopicId, group.TopicId);
            }
            //发送邀请
            if (!string.IsNullOrEmpty(groupEditModel.RelatedUserIds))
            {

                //已修改
                IEnumerable<long> userIds = Request.Form.Gets<long>("RelatedUserIds", null);
                topicService.SendInvitations(group, user, string.Empty, userIds);
            }
            return Redirect(SiteUrls.Instance().TopicHome(group.TopicKey));
        }

        #region 专题全文检索

        /// <summary>
        /// 专题搜索
        /// </summary>
        public ActionResult Search(TopicFullTextQuery query)
        {
            query.Keyword = WebUtility.UrlDecode(query.Keyword);
            query.PageSize = 20;//每页记录数

            //调用搜索器进行搜索
            TopicSearcher groupSearcher = (TopicSearcher)SearcherFactory.GetSearcher(TopicSearcher.CODE);
            PagingDataSet<TopicEntity> groups = groupSearcher.Search(query);

            //添加到用户搜索历史 
            IUser CurrentUser = UserContext.CurrentUser;
            if (CurrentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    SearchHistoryService searchHistoryService = new SearchHistoryService();
                    searchHistoryService.SearchTerm(CurrentUser.UserId, TopicSearcher.CODE, query.Keyword);
                }
            }

            //添加到热词
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                SearchedTermService searchedTermService = new SearchedTermService();
                searchedTermService.SearchTerm(TopicSearcher.CODE, query.Keyword);
            }

            //设置页面Meta
            if (string.IsNullOrWhiteSpace(query.Keyword))
            {
                pageResourceManager.InsertTitlePart("专题搜索");//设置页面Title
            }
            else
            {
                pageResourceManager.InsertTitlePart('“' + query.Keyword + '”' + "的相关专题");//设置页面Title
            }

            return View(groups);
        }

        /// <summary>
        /// 专题全局搜索
        /// </summary>
        public ActionResult _GlobalSearch(TopicFullTextQuery query, int topNumber)
        {
            query.PageSize = topNumber;//每页记录数
            query.PageIndex = 1;

            //调用搜索器进行搜索
            TopicSearcher groupSearcher = (TopicSearcher)SearcherFactory.GetSearcher(TopicSearcher.CODE);
            PagingDataSet<TopicEntity> groups = groupSearcher.Search(query);

            return PartialView(groups);
        }

        /// <summary>
        /// 专题快捷搜索
        /// </summary>
        public ActionResult _QuickSearch(TopicFullTextQuery query, int topNumber)
        {
            query.PageSize = topNumber;//每页记录数
            query.PageIndex = 1;
            query.Range = TopicSearchRange.GROUPNAME;
            query.Keyword = Server.UrlDecode(query.Keyword);
            //调用搜索器进行搜索
            TopicSearcher TopicSearcher = (TopicSearcher)SearcherFactory.GetSearcher(TopicSearcher.CODE);
            PagingDataSet<TopicEntity> groups = TopicSearcher.Search(query);

            return PartialView(groups);
        }

        /// <summary>
        /// 专题搜索自动完成
        /// </summary>
        public JsonResult SearchAutoComplete(string keyword, int topNumber)
        {
            //调用搜索器进行搜索
            TopicSearcher groupSearcher = (TopicSearcher)SearcherFactory.GetSearcher(TopicSearcher.CODE);
            IEnumerable<string> terms = groupSearcher.AutoCompleteSearch(keyword, topNumber);

            var jsonResult = Json(terms.Select(t => new { tagName = t, tagNameWithHighlight = SearchEngine.Highlight(keyword, string.Join("", t.Take(34)), 100) }), JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        /// <summary>
        /// 可能感兴趣的专题
        /// </summary>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _InterestTopic()
        {
            TagService tagService = new TagService(TenantTypeIds.Instance().User());
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                TopicFullTextQuery query = new TopicFullTextQuery();
                query.PageSize = 20;
                query.PageIndex = 1;
                query.Range = TopicSearchRange.TAG;
                query.Tags = tagService.GetTopTagsOfItem(currentUser.UserId, 100).Select(n => n.TagName);
                query.TopicIds = topicService.GetMyJoinedTopics(currentUser.UserId, 100, 1).Select(n => n.TopicId.ToString());
                //调用搜索器进行搜索
                TopicSearcher TopicSearcher = (TopicSearcher)SearcherFactory.GetSearcher(TopicSearcher.CODE);
                IEnumerable<TopicEntity> groupsTag = null;
                if (TopicSearcher.Search(query, true).Count == 0)
                {
                    return View();
                }
                else
                {
                    groupsTag = TopicSearcher.Search(query, true).AsEnumerable<TopicEntity>();
                }
                if (groupsTag.Count() < 20)
                {
                    IEnumerable<TopicEntity> groupsFollow = topicService.FollowedUserAlsoJoinedTopics(currentUser.UserId, 20 - groupsTag.Count());
                    return View(groupsTag.Union(groupsFollow));
                }
                else
                {
                    return View(groupsTag);
                }
            }
            else
            {
                return View();
            }
        }

        #endregion

        #region 动态内容块
        /// <summary>
        /// 创建专题动态内容块
        /// </summary>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreateTopic(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
                return Content(string.Empty);
            TopicEntity group = topicService.Get(activity.SourceId);
            if (group == null)
                return Content(string.Empty);
            ViewData["ActivityId"] = ActivityId;
            return View(group);
        }

        /// <summary>
        /// 用户加入专题动态内容快
        /// </summary>
        /// <param name="ActivityId">动态id</param>
        /// <returns>用户加入专题动态内容快</returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreateTopicMember(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
                return Content(string.Empty);

            TopicEntity group = topicService.Get(activity.OwnerId);
            if (group == null)
                return Content(string.Empty);

            IEnumerable<TopicMember> groupMembers = topicService.GetTopicMembers(group.TopicId, true, SortBy_TopicMember.DateCreated_Desc);
            ViewData["activity"] = activity;
            ViewData["TopicMembers"] = groupMembers;
            return View(group);
        }

        /// <summary>
        /// 用户加入专题动态内容快
        /// </summary>
        /// <param name="ActivityId">动态id</param>
        /// <returns>用户加入专题动态内容快</returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _JoinTopic(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
                return Content(string.Empty);
            TopicMember groupMember = topicService.GetTopicMember(activity.SourceId);
            if (groupMember == null)
                return Content(string.Empty);
            TopicEntity group = topicService.Get(groupMember.TopicId);
            if (group == null)
                return Content(string.Empty);

            ViewData["activity"] = activity;
            return View(group);
        }

        #endregion

        #region 屏蔽专题

        /// <summary>
        /// 屏蔽专题
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BlockTopic(long groupId)
        {
            TopicEntity blockedTopic = topicService.Get(groupId);
            if (blockedTopic == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到被屏蔽专题"));
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您还没有登录"));
            new UserBlockService().BlockTopic(currentUser.UserId, blockedTopic.TopicId, blockedTopic.TopicName);
            return Json(new StatusMessageData(StatusMessageType.Success, "操作成功"));
        }

        /// <summary>
        /// 屏蔽专题的post方法
        /// </summary>
        /// <param name="spaceKey">屏蔽的spacekey</param>
        /// <param name="groupIds">被屏蔽的分组名称</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BlockTopics(string spaceKey, List<long> groupIds)
        {
            int addCount = 0;

            long userId = UserIdToUserNameDictionary.GetUserId(spaceKey);
            UserBlockService service = new UserBlockService();

            if (userId > 0 && groupIds != null && groupIds.Count > 0)
                foreach (var groupId in groupIds)
                {
                    TopicEntity group = topicService.Get(groupId);
                    if (group == null || service.IsBlockedTopic(userId, groupId))
                        continue;
                    service.BlockTopic(userId, group.TopicId, group.TopicName);
                    addCount++;
                }
            if (addCount > 0)
                TempData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Success, string.Format("成功添加{0}个专题添加到屏蔽列表", addCount));
            else
                TempData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Error, "没有任何专题被添加到屏蔽列表中");
            return Redirect(SiteUrls.Instance().BlockGroups(spaceKey));
        }

        /// <summary>
        /// 屏蔽专题
        /// </summary>
        /// <param name="spaceKey">空间名</param>
        /// <returns>屏蔽专题名</returns>
        public ActionResult _BlockTopics(string spaceKey)
        {
            long userId = UserIdToUserNameDictionary.GetUserId(spaceKey);
            if (UserContext.CurrentUser == null || (UserContext.CurrentUser.UserId != userId && authorizer.IsAdministrator(new TenantTypeService().Get(TenantTypeIds.Instance().Topic()).ApplicationId)))
                return Content(string.Empty);

            IEnumerable<UserBlockedObject> blockedTopics = new UserBlockService().GetBlockedTopics(userId);

            List<UserBlockedObjectViewModel> blockedObjectes = new List<UserBlockedObjectViewModel>();

            if (blockedTopics != null && blockedTopics.Count() > 0)
            {
                topicService.GetTopicEntitiesByIds(blockedTopics.Select(n => n.ObjectId));
                foreach (var item in blockedTopics)
                {
                    TopicEntity group = topicService.Get(item.ObjectId);
                    if (group == null)
                        continue;

                    UserBlockedObjectViewModel entitiy = item.AsViewModel();
                    entitiy.Logo = group.Logo;
                    blockedObjectes.Add(entitiy);
                }
            }

            return View(blockedObjectes);
        }

        #endregion

        #region 加入专题
        /// <summary>
        /// 申请加入按钮
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>   
        [HttpGet]
        public ActionResult _ApplyJoinButton(long groupId, bool showQuit = false, string buttonName = null)
        {

            TopicEntity group = topicService.Get(groupId);
            if (group == null)
                return new EmptyResult();
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return new EmptyResult();
            bool isApplied = topicService.IsApplied(currentUser.UserId, groupId);
            bool isMember = topicService.IsMember(group.TopicId, currentUser.UserId);
            bool isOwner = topicService.IsOwner(group.TopicId, currentUser.UserId);
            bool isManager = topicService.IsManager(group.TopicId, currentUser.UserId);
            ViewData["isMember"] = isMember;
            ViewData["showQuit"] = showQuit;
            ViewData["buttonName"] = buttonName;
            ViewData["isOwner"] = isOwner;
            ViewData["isManager"] = isManager;
            ViewData["isApplied"] = isApplied;
            return View(group);
        }

        /// <summary>
        /// 退出专题
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _QuitTopic(long groupId)
        {
            StatusMessageData message = new StatusMessageData(StatusMessageType.Success, "退出专题成功！");
            TopicEntity group = topicService.Get(groupId);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            try
            {
                topicService.DeleteTopicMember(group.TopicId, currentUser.UserId);
            }
            catch
            {
                message = new StatusMessageData(StatusMessageType.Error, "退出专题失败！");
            }
            return Json(message);
        }

        /// <summary>
        /// 用户加入专题（专题无验证时）
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult JoinTopic(long groupId)
        {
            //需判断是否已经加入过专题
            StatusMessageData message = null;
            TopicEntity group = topicService.Get(groupId);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            if (group.JoinWay != JoinWay.Direct)
                return Json(new StatusMessageData(StatusMessageType.Error, "当前加入方式不是直接加入"));

            //已修改

            //判断是否加入过该专题
            bool isMember = topicService.IsMember(groupId, currentUser.UserId);

            //未加入
            if (!isMember)
            {
                TopicMember member = TopicMember.New();
                member.UserId = currentUser.UserId;
                member.TopicId = group.TopicId;
                member.IsManager = false;
                topicService.CreateTopicMember(member);
                message = new StatusMessageData(StatusMessageType.Success, "加入专题成功！");
            }
            else
            {
                message = new StatusMessageData(StatusMessageType.Hint, "您已加入过该专题！");
            }
            return Json(message);
        }

        /// <summary>
        /// 用户加入专题（专题有验证时）
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _EditApply(long groupId)
        {

            //已修改

            bool isApplied = topicService.IsApplied(UserContext.CurrentUser.UserId, groupId);
            ViewData["isApplied"] = isApplied;
            return View();
        }

        /// <summary>
        /// 用户加入专题（专题有验证时）
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _EditApply(long groupId, string applyReason)
        {
            StatusMessageData message = null;
            TopicEntity group = topicService.Get(groupId);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            if (group.JoinWay != JoinWay.ByApply)
                return Json(new StatusMessageData(StatusMessageType.Error, "当前加入方式不是需要申请"));


            //已修改
            bool isApplied = topicService.IsApplied(currentUser.UserId, group.TopicId);
            if (!isApplied)
            {
                TopicMemberApply apply = TopicMemberApply.New();
                apply.ApplyReason = applyReason;
                apply.ApplyStatus = TopicMemberApplyStatus.Pending;
                apply.TopicId = group.TopicId;
                apply.UserId = UserContext.CurrentUser.UserId;
                topicService.CreateTopicMemberApply(apply);
                message = new StatusMessageData(StatusMessageType.Success, "申请已发出，请等待！");
            }
            else
            {
                message = new StatusMessageData(StatusMessageType.Hint, "您已给该专题发送过申请！");
            }
            return Json(message);
        }

        /// <summary>
        ///  用户加入专题（通过问题验证）
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _ValidateQuestion(long groupId)
        {
            TopicEntity group = topicService.Get(groupId);
            ViewData["Question"] = group.Question;
            return View();
        }

        /// <summary>
        /// 用户加入专题（通过问题验证）
        /// </summary>
        /// <param name="groupId">专题Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _ValidateQuestion(long groupId, string myAnswer)
        {
            StatusMessageData message = null;
            TopicEntity group = topicService.Get(groupId);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到专题！"));


            //已修改
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            if (group.JoinWay != JoinWay.ByQuestion)
                return Json(new StatusMessageData(StatusMessageType.Error, "当前加入方式不是问题验证"));


            bool isMember = topicService.IsMember(group.TopicId, currentUser.UserId);
            if (!isMember)
            {
                if (group.Answer == myAnswer)
                {
                    TopicMember member = TopicMember.New();
                    member.UserId = UserContext.CurrentUser.UserId;
                    member.TopicId = group.TopicId;
                    member.IsManager = false;
                    topicService.CreateTopicMember(member);
                    message = new StatusMessageData(StatusMessageType.Success, "加入专题成功！");
                }
                else
                {
                    message = new StatusMessageData(StatusMessageType.Error, "答案错误！");
                }
            }
            else
            {
                message = new StatusMessageData(StatusMessageType.Hint, "您已加入过该专题！");
            }
            return Json(message);
        }

        #endregion

        #region 推荐专题
        /// <summary>
        /// 推荐专题
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _RecommendedTopic()
        {
            IEnumerable<RecommendItem> recommendItems = recommendService.GetTops(6, "90020001");
            return View(recommendItems);
        }
        #endregion

        #region 页面

        /// <summary>
        /// 发现专题
        /// </summary>
        /// <returns></returns>
        public ActionResult FindTopic(string nameKeyword, string areaCode, long? categoryId, SortBy_Topic? sortBy, int pageIndex = 1)
        {
            nameKeyword = WebUtility.UrlDecode(nameKeyword);
            string pageTitle = string.Empty;
            IEnumerable<Category> childCategories = null;
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var category = categoryService.Get(categoryId.Value);
                if (category != null)
                {


                    if (category.ChildCount > 0)
                    {
                        childCategories = category.Children;
                    }
                    else//若是叶子节点，则取同辈分类
                    {
                        if (category.Parent != null)
                            childCategories = category.Parent.Children;
                    }
                    List<Category> allParentCategories = new List<Category>();
                    //递归获取所有父级类别，若不是叶子节点，则包含其自身
                    RecursiveGetAllParentCategories(category.ChildCount > 0 ? category : category.Parent, ref allParentCategories);
                    ViewData["allParentCategories"] = allParentCategories;
                    ViewData["currentCategory"] = category;
                    pageTitle = category.CategoryName;
                }
            }


            if (childCategories == null)
                childCategories = categoryService.GetRootCategories(TenantTypeIds.Instance().Topic());

            ViewData["childCategories"] = childCategories;

            AreaSettings areaSettings = DIContainer.Resolve<ISettingsManager<AreaSettings>>().Get();
            IEnumerable<Area> childArea = null;
            if (!string.IsNullOrEmpty(areaCode))
            {
                var area = areaService.Get(areaCode);
                if (area != null)
                {


                    if (area.ChildCount > 0)
                    {
                        childArea = area.Children;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(area.ParentCode))
                        {
                            var parentArea = areaService.Get(area.ParentCode);
                            if (parentArea != null)
                                childArea = parentArea.Children;
                        }
                    }
                }
                List<Area> allParentAreas = new List<Area>();
                RecursiveGetAllParentArea(area.ChildCount > 0 ? area : areaService.Get(area.ParentCode), areaSettings.RootAreaCode, ref allParentAreas);
                ViewData["allParentAreas"] = allParentAreas;
                ViewData["currentArea"] = area;
                if (!string.IsNullOrEmpty(pageTitle))
                    pageTitle += ",";
                pageTitle += area.Name;
            }

            if (childArea == null)
            {
                Area rootArea = areaService.Get(areaSettings.RootAreaCode);
                if (rootArea != null)
                    childArea = rootArea.Children;
                else
                    childArea = areaService.GetRoots();
            }

            ViewData["childArea"] = childArea;

            if (!string.IsNullOrEmpty(nameKeyword))
            {
                if (!string.IsNullOrEmpty(pageTitle))
                    pageTitle += ",";
                pageTitle += nameKeyword;
            }

            if (string.IsNullOrEmpty(pageTitle))
                pageTitle = "发现专题";
            pageResourceManager.InsertTitlePart(pageTitle);
            PagingDataSet<TopicEntity> groups = topicService.Gets(areaCode, categoryId, sortBy ?? SortBy_Topic.DateCreated_Desc, pageIndex: pageIndex);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_List", groups);
            }

            return View(groups);

        }
        /// <summary>
        /// 迭代获取类别
        /// </summary>
        /// <param name="category"></param>
        /// <param name="allParentCategories"></param>
        private void RecursiveGetAllParentCategories(Category category, ref List<Category> allParentCategories)
        {
            if (category == null)
                return;
            allParentCategories.Insert(0, category);
            Category parent = category.Parent;
            if (parent != null)
                RecursiveGetAllParentCategories(parent, ref allParentCategories);
        }
        /// <summary>
        /// 迭代获取地区
        /// </summary>
        /// <param name="area"></param>
        /// <param name="rootAreaCode"></param>
        /// <param name="allParentAreas"></param>
        private void RecursiveGetAllParentArea(Area area, string rootAreaCode, ref List<Area> allParentAreas)
        {
            if (area == null || area.AreaCode == rootAreaCode)
                return;


            allParentAreas.Insert(0, area);
            Area parent = areaService.Get(area.ParentCode);
            if (parent != null)
            {
                RecursiveGetAllParentArea(parent, rootAreaCode, ref allParentAreas);
            }
        }


        /// <summary>
        /// 用户的专题页
        /// </summary>
        /// <returns></returns>
        [UserSpaceAuthorize]
        public ActionResult UserJoinedTopics(string spaceKey, int pageIndex = 1)
        {
            string title = "我加入的专题";
            IUserService userService = DIContainer.Resolve<IUserService>();
            User spaceUser = userService.GetFullUser(spaceKey);
            var currentUser = UserContext.CurrentUser;
            if (spaceUser == null)
                return HttpNotFound();


            if (currentUser != null)
            {
                if (currentUser.UserId != spaceUser.UserId)
                {
                    title = spaceUser.DisplayName + "加入的专题";
                }
            }
            else
            {
                title = spaceUser.DisplayName + "加入的专题";
            }


            PagingDataSet<TopicEntity> groups = topicService.GetMyJoinedTopics(spaceUser.UserId, pageIndex: pageIndex);
            if (Request.IsAjaxRequest())
                return PartialView("_List", groups);

            ViewData["spaceUser"] = spaceUser;
            ViewData["currentUser"] = currentUser;
            pageResourceManager.InsertTitlePart(title);

            #region 身份认证
            List<Identification> identifications = identificationService.GetUserIdentifications(spaceUser.UserId);
            if (identifications.Count() > 0)
            {
                ViewData["identificationTypeVisiable"] = true;
            }
            #endregion



            //设置当前登录用户对当前页用户的关注情况





            return View(groups);
        }



        /// <summary>
        /// 用户创建的专题页
        /// </summary>
        /// <returns></returns>
        [UserSpaceAuthorize]
        public ActionResult UserCreatedTopics(string spaceKey)
        {
            string title = "我创建的专题";
            IUserService userService = DIContainer.Resolve<IUserService>();
            User spaceUser = userService.GetFullUser(spaceKey);
            if (spaceUser == null)
                return HttpNotFound();
            bool ignoreAudit = false;
            var currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                if (currentUser.UserId == spaceUser.UserId || authorizer.IsAdministrator(TopicConfig.Instance().ApplicationId))
                    ignoreAudit = true;

                if (currentUser.UserId != spaceUser.UserId)
                {
                    title = spaceUser.DisplayName + "创建的专题";
                }
            }
            else
            {
                title = spaceUser.DisplayName + "创建的专题";
            }

            pageResourceManager.InsertTitlePart(title);
            var groups = topicService.GetMyCreatedTopics(spaceUser.UserId, ignoreAudit);
            if (Request.IsAjaxRequest())
                return PartialView("_List", groups);

            ViewData["spaceUser"] = spaceUser;
            ViewData["currentUser"] = currentUser;


            return View(groups);
        }

        /// <summary>
        /// 标签显示专题列表
        /// </summary>
        public ActionResult ListByTag(string tagName, SortBy_Topic sortBy = SortBy_Topic.DateCreated_Desc, int pageIndex = 1)
        {
            tagName = WebUtility.UrlDecode(tagName);
            var tag = new TagService(TenantTypeIds.Instance().Topic()).Get(tagName);

            if (tag == null)
            {
                return HttpNotFound();
            }

            PagingDataSet<TopicEntity> groups = topicService.GetsByTag(tagName, sortBy, pageIndex: pageIndex);
            pageResourceManager.InsertTitlePart(tagName);
            ViewData["tag"] = tag;
            ViewData["sortBy"] = sortBy;
            return View(groups);
        }

        #endregion

        #region 内容块


        /// <summary>
        /// 顶部专题导航
        /// </summary>
        /// <returns></returns>
        public ActionResult _TopicGlobalNavigations()
        {
            IUser CurrentUser = UserContext.CurrentUser;
            IEnumerable<TopicEntity> groups = null;
            if (CurrentUser != null)
            {
                groups = topicService.GetMyCreatedTopics(CurrentUser.UserId, true);

                if (groups.Count() >= 9)
                    return View(groups.Take(9));

                PagingDataSet<TopicEntity> joinedTopics = topicService.GetMyJoinedTopics(CurrentUser.UserId);
                groups = groups.Union(joinedTopics).Take(9);
            }

            return View(groups);
        }

        /// <summary>
        /// 专题排行内容块
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _TopTopics(int topNumber, string areaCode, long? categoryId, SortBy_Topic? sortBy, string viewName = "_TopTopics_List")
        {
            var groups = topicService.GetTops(topNumber, areaCode, categoryId, sortBy ?? SortBy_Topic.DateCreated_Desc);



            ViewData["SortBy"] = sortBy;
            return PartialView(viewName, groups);
        }


        /// <summary>
        /// 专题分类导航内容块（包含1、2级）
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _CategoryTopics()
        {
            IEnumerable<Category> categories = categoryService.GetRootCategories(TenantTypeIds.Instance().Topic());
            return PartialView(categories);
        }

        /// <summary>
        /// 专题地区导航内容块
        /// </summary>
        /// <returns></returns>
        public ActionResult _AreaTopics(int topNumber, string areaCode, long? categoryId, SortBy_Topic sortBy = SortBy_Topic.DateCreated_Desc)
        {
            IUser iUser = (User)UserContext.CurrentUser;
            User user = null;
            if (iUser == null)
            {
                user = new User();
            }
            else
            {
                user = userService.GetFullUser(iUser.UserId);
            }
            if (string.IsNullOrEmpty(areaCode) && Request.Cookies["AreaTopicCookie" + user.UserId] != null && !string.IsNullOrEmpty(Request.Cookies["AreaTopicCookie" + user.UserId].Value))
                areaCode = Request.Cookies["AreaTopicCookie" + user.UserId].Value;

            if (string.IsNullOrEmpty(areaCode))
            {
                string ip = WebUtility.GetIP();
                areaCode = IPSeeker.Instance().GetAreaCode(ip);
                if (string.IsNullOrEmpty(areaCode) && user.Profile != null)
                {
                    areaCode = user.Profile.NowAreaCode;
                }
            }
            ViewData["areaCode"] = areaCode;
            if (!string.IsNullOrEmpty(areaCode))
            {
                Area area = areaService.Get(areaCode);
                if (!string.IsNullOrEmpty(area.ParentCode))
                {
                    Area parentArea = areaService.Get(area.ParentCode);
                    ViewData["parentCode"] = parentArea.AreaCode;
                }
            }

            IEnumerable<TopicEntity> groups = topicService.GetTops(topNumber, areaCode, categoryId, sortBy);

            HttpCookie cookie = new HttpCookie("AreaTopicCookie" + user.UserId, areaCode);
            Response.Cookies.Add(cookie);

            return PartialView(groups);
        }

        /// <summary>
        /// 人气群主
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _RecommendedTopicOwners(int topNumber = 5, string recommendTypeId = null)
        {
            IEnumerable<RecommendItem> recommendUsers = recommendService.GetTops(topNumber, recommendTypeId);


            return PartialView(recommendUsers);
        }
        /// <summary>
        /// 标签地图
        /// </summary>
        /// <returns></returns>
        public ActionResult TopicTagMap()
        {
            pageResourceManager.InsertTitlePart("标签云图");
            return View();
        }
        /// <summary>
        /// 关注按钮
        /// </summary>
        /// <param name="currentUser">当前用户</param>
        /// <param name="followedUser">要关注的用户</param>
        /// <returns></returns>
        public ActionResult _FollowedButton(User currentUser, User followedUser)
        {
            ViewData["currentUser"] = currentUser;
            ViewData["followedUser"] = followedUser;
            if (currentUser != null && currentUser.UserId != followedUser.UserId)
            {
                bool currentUserIsFollowedUser = false;
                ViewData["currentUserIsFollowedUser"] = currentUserIsFollowedUser;
            }
            bool isFollowed = currentUser.IsFollowed(followedUser.UserId);
            ViewData["isFollowed"] = isFollowed;

            return View();
        }

        #endregion

    }
}
