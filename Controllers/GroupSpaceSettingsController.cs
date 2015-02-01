//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Common.Configuration;

using Tunynet.UI;
using Tunynet.Utilities;

namespace SpecialTopic.Topic.Controllers
{
    [Themed(PresentAreaKeysOfBuiltIn.GroupSpace, IsApplication = true)]
    [AnonymousBrowseCheck]
    [TitleFilter(IsAppendSiteName = true)]
    [TopicSpaceAuthorize(RequireManager = true)]
    public class GroupSpaceSettingsController : Controller
    {

        public IPageResourceManager pageResourceManager { get; set; }
        public CategoryService categoryService { get; set; }
        public TopicService groupService { get; set; }
        public Authorizer authorizer { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().Group());

        
        //这个多个地方用到
        /// <summary>
        /// 右侧导航菜单
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public ActionResult _GroupSettingRightMenu(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            PagingDataSet<TopicMemberApply> applys = groupService.GetTopicMemberApplies(group.GroupId, TopicMemberApplyStatus.Pending);
            long totalRecords = applys.TotalRecords;
            ViewData["totalRecords"] = totalRecords;
            return View(group);
        }

        /// <summary>
        /// 管理群组成员申请页
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ManageMemberApplies(string spaceKey, TopicMemberApplyStatus? applyStatus, int pageIndex = 1, int pageSize = 20)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            pageResourceManager.InsertTitlePart(group.GroupName);
            pageResourceManager.InsertTitlePart("管理群组成员申请页");
            
            //已修改
            PagingDataSet<TopicMemberApply> groupMemberApplies = groupService.GetTopicMemberApplies(group.GroupId, applyStatus, pageSize, pageIndex);
            ViewData["groupId"] = group.GroupId;
            TempData["GroupMenu"] = GroupMenu.ManageMember;

            return View(groupMemberApplies);
        }

        /// <summary>
        /// 接受/拒绝群组加入申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ApproveMemberApply(string spaceKey, IList<long> applyIds, bool isApproved)
        {
            
            
            long groupId = TopicIdToTopicKeyDictionary.GetGroupId(spaceKey);
            groupService.ApproveTopicMemberApply(applyIds, isApproved);
            return Json(new StatusMessageData(StatusMessageType.Success, "操作成功"));
        }


        /// <summary>
        /// 删除群组加入申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteMemberApply(string spaceKey, long id)
        {
            
            
            long groupId = TopicIdToTopicKeyDictionary.GetGroupId(spaceKey);
            groupService.DeleteTopicMemberApply(id);
            return Json(new StatusMessageData(StatusMessageType.Success, "操作成功"));
        }

        /// <summary>
        /// 管理群组成员页
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ManageMembers(string spaceKey, int pageIndex = 1, int pageSize = 20)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            pageResourceManager.InsertTitlePart(group.GroupName);
            pageResourceManager.InsertTitlePart("管理群组成员页");




            PagingDataSet<TopicMember> groupMembers = groupService.GetTopicMembers(group.GroupId, true, SortBy_TopicMember.DateCreated_Asc, pageSize, pageIndex);
            ViewData["group"] = group;
            TempData["GroupMenu"] = GroupMenu.ManageMember;

            return View(groupMembers);
        }

        
        

        
        
        
        /// <summary>
        /// 创建更换群主模式框
        /// </summary>
        /// <param name="groupId">群组Id</param>
        /// <param name="userId">群主名称</param>
        /// <returns>更换群主</returns>
        [HttpGet]
        public ActionResult _ChangeGroupOwner(string spaceKey, string returnUrl)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return Content(string.Empty);
            
            
            
            ViewData["returnUrl"] = WebUtility.UrlDecode(returnUrl);
            return View(group);
        }

        
        
        /// <summary>
        /// 更换群主
        /// </summary>
        /// <param name="group">编辑群组对象</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _ChangeGroupOwner(string spaceKey)
        {
            string returnUrl = Request.QueryString.Get<string>(WebUtility.UrlDecode("returnUrl"));
            
            var userIds = Request.Form.Gets<long>("UserId", new List<long>());
            long userId = userIds.FirstOrDefault();
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return Content(string.Empty);
            if (userId == 0)
            {
                Tunynet.Utilities.WebUtility.SetStatusCodeForError(Response);
                ViewData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Hint, "您没有选择群主");
                ViewData["returnUrl"] = returnUrl;
                return View(group);
            }
            if (group.UserId == userId)
            {
                Tunynet.Utilities.WebUtility.SetStatusCodeForError(Response);
                ViewData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Hint, "您没有更换群主");
                ViewData["returnUrl"] = returnUrl;
                return View(group);
            }
            if (!authorizer.Group_SetManager(group))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有更换群主的权限"));
            }

            groupService.ChangeGroupOwner(group.GroupId, userId);
            return Json(new StatusMessageData(StatusMessageType.Success, "更换群主操作成功"));
        }

        /// <summary>
        ///  设置/取消 群组管理员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetManager(string spaceKey, long userId, bool isManager)
        {
            StatusMessageData message = null;
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();

            if (!authorizer.Group_SetManager(group))
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有设置管理员的权限"));

            bool result = groupService.SetManager(group.GroupId, userId, isManager);
            if (result)
            {
                message = new StatusMessageData(StatusMessageType.Success, "操作成功！");
            }
            else
            {
                message = new StatusMessageData(StatusMessageType.Error, "操作失败！");
            }
            return Json(message);
        }

        
        
        /// <summary>
        /// 批量移除群组成员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteMember(string spaceKey, List<long> userIds)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            foreach (var userId in userIds)
            {
                if (!authorizer.Group_DeleteMember(group, userId))
                {
                    return Json(new StatusMessageData(StatusMessageType.Error, "您没有删除群组成员的权限"));
                }
            }




            groupService.DeleteTopicMember(group.GroupId, userIds);
            return Json(new StatusMessageData(StatusMessageType.Success, "操作成功"));
        }

        /// <summary>
        /// 删除群组logo
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeleteGroupLogo(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            if (group == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "没有该群组！"));
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            
            //已修改
            //这个功能属于编辑群组，在编辑群组已做权限验证，这边还需要做验证吗？
            
            groupService.DeleteLogo(group.GroupId);
            return Json(new StatusMessageData(StatusMessageType.Success, "删除群组Logo成功！"));
        }

        /// <summary>
        /// 编辑群组页
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditGroup(string spaceKey)
        {
            TopicEntity group = groupService.Get(spaceKey);
            
            //已修改
            if (group == null)
                return HttpNotFound();
            pageResourceManager.InsertTitlePart(group.GroupName);
            pageResourceManager.InsertTitlePart("编辑群组");

            
            //编辑的时候需要显示已添加的标签
            IEnumerable<string> tags = group.TagNames;
            GroupEditModel groupEditModel = group.AsEditModel();
            ViewData["tags"] = tags;
            TempData["GroupMenu"] = GroupMenu.GroupSettings;

            return View(groupEditModel);
        }

        /// <summary>
        /// 编辑群组
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditGroup(string spaceKey, GroupEditModel groupEditModel)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "您尚未登录！"));
            System.IO.Stream stream = null;
            HttpPostedFileBase groupLogo = Request.Files["GroupLogo"];

            if (groupLogo != null && !string.IsNullOrEmpty(groupLogo.FileName))
            {
                TenantLogoSettings tenantLogoSettings = TenantLogoSettings.GetRegisteredSettings(TenantTypeIds.Instance().Group());
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

            TopicEntity group = groupEditModel.AsGroupEntity();


            //设置分类
            if (groupEditModel.CategoryId > 0)
            {
                categoryService.ClearCategoriesFromItem(group.GroupId, 0, TenantTypeIds.Instance().Group());
                categoryService.AddItemsToCategory(new List<long>() { group.GroupId }, groupEditModel.CategoryId);
            }

            
            //已修改
            //设置标签
            string relatedTags = Request.Form.Get<string>("RelatedTags");
            if (!string.IsNullOrEmpty(relatedTags))
            {
                tagService.ClearTagsFromItem(group.GroupId, group.GroupId);
                tagService.AddTagsToItem(relatedTags, group.GroupId, group.GroupId);
            }
            if (stream != null)
            {
                groupService.UploadLogo(group.GroupId, stream);
            }

            groupService.Update(currentUser.UserId, group);
            TempData["StatusMessageData"] = new StatusMessageData(StatusMessageType.Success, "更新成功！");
            return Redirect(SiteUrls.Instance().EditGroup(group.GroupKey));
        }

        [HttpGet]
        public ActionResult _Menu_Manage(string spaceKey)
        {
            long groupId = TopicIdToTopicKeyDictionary.GetGroupId(spaceKey);
            TopicEntity group = groupService.Get(groupId);
            if (group == null)
                return Content(string.Empty);

            int currentNavigationId = RouteData.Values.Get<int>("CurrentNavigationId", 0);

            NavigationService navigationService = new NavigationService();
            Navigation navigation = navigationService.GetNavigation(PresentAreaKeysOfBuiltIn.GroupSpace, currentNavigationId, group.GroupId);

            IEnumerable<Navigation> navigations = new List<Navigation>();
            if (navigation != null)
            {
                if (navigation.Depth >= 1 && navigation.Parent != null)
                {
                    navigations = navigation.Parent.Children;
                }
                else if (navigation.Depth == 0)
                {
                    navigations = navigation.Children;
                }
            }

            ViewData["MemberApplyCount"] = groupService.GetMemberApplyCount(group.GroupId);

            return View(navigations);
        }
    }
}