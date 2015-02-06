//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using System.ComponentModel.DataAnnotations;

using Spacebuilder.Common;
using System.Web.Mvc;


namespace SpecialTopic.Topic
{
    /// <summary>
    /// 编辑专题实体
    /// </summary>
    public class TopicEditModel
    {
        /// <summary>
        ///TopicId
        /// </summary>
        public long TopicId { get; set; }

        /// <summary>
        ///专题名称
        /// </summary>
        [Display(Name = "名称")]
        [WaterMark(Content = "在此输入专题名称")]
        [Required(ErrorMessage = "请输入专题名称")]
        [StringLength(60, ErrorMessage = "最多允许输入60个字")]
        [DataType(DataType.Text)]
        public string TopicName { get; set; }

        /// <summary>
        ///专题标识（个性网址的关键组成部分）
        /// </summary>
        [Required(ErrorMessage = "请输入专题标识")]
        [StringLength(16, MinimumLength = 4, ErrorMessage = "请输入4-16个字")]
        [DataType(DataType.Url)]
        [Remote("ValidateTopicKey", "ChannelTopic", "Topic", ErrorMessage = "此专题Key已存在", AdditionalFields = "TopicId")]
        public string TopicKey { get; set; }

        /// <summary>
        ///专题介绍
        /// </summary>
        [Display(Name = "简介")]
        [StringLength(300, ErrorMessage = "最多可以输入300个字")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        /// <summary>
        ///所在地区
        /// </summary>        
        public string AreaCode { get; set; }

        /// <summary>
        ///logo名称（带部分路径）
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        ///是否公开
        /// </summary>
        [Required(ErrorMessage = "请选择专题类型")]
        public bool IsPublic { get; set; }

        /// <summary>
        ///加入方式
        /// </summary>
        [Required(ErrorMessage = "请选择加入方式")]
        public TopicJoinWay JoinWay { get; set; }

        /// <summary>
        ///是否允许成员邀请（一直允许群管理员邀请）
        /// </summary>
        public bool EnableMemberInvite { get; set; }

        /// <summary>
        /// 问题
        /// </summary>
        [WaterMark(Content = "如1+1=？")]
        [StringLength(36, ErrorMessage = "最多输入36个汉字")]
        [DataType(DataType.Text)]
        public string Question { get; set; }

        /// <summary>
        /// 答案
        /// </summary>
        [StringLength(36, ErrorMessage = "最多输入36个汉字")]
        [DataType(DataType.Text)]
        public string Answer { get; set; }

        /// <summary>
        /// 分类Id
        /// </summary>
        [Required(ErrorMessage = "请选择类别")]
        public long CategoryId { get; set; }

        /// <summary>
        /// 相关用户Id集合
        /// </summary>
        public string RelatedUserIds { get; set; }

        /// <summary>
        /// 相关标签集合
        /// </summary>
        public string[] RelatedTags { get; set; }

        /// <summary>
        /// 转换成groupEntity类型
        /// </summary>
        /// <returns></returns>
        public TopicEntity AsTopicEntity()
        {
            CategoryService categoryService = new CategoryService();
            TopicEntity groupEntity = null;

            //创建专题
            if (this.TopicId == 0)
            {
                groupEntity = TopicEntity.New();
                groupEntity.UserId = UserContext.CurrentUser.UserId;
                groupEntity.DateCreated = DateTime.UtcNow;
                groupEntity.TopicKey = this.TopicKey;
            }
            //编辑专题
            else
            {
                TopicService groupService = new TopicService();
                groupEntity = groupService.Get(this.TopicId);
            }
            groupEntity.IsPublic = this.IsPublic;
            groupEntity.TopicName = this.TopicName;
            if (Logo != null)
            {
                groupEntity.Logo = this.Logo;
            }
            groupEntity.Description = Formatter.FormatMultiLinePlainTextForStorage(this.Description == null ? string.Empty : this.Description, true);
            groupEntity.AreaCode = this.AreaCode??string.Empty;
            groupEntity.JoinWay = this.JoinWay;
            groupEntity.EnableMemberInvite = this.EnableMemberInvite;
            if (JoinWay == TopicJoinWay.ByQuestion)
            {
                groupEntity.Question = this.Question;
                groupEntity.Answer = this.Answer;
            }
            return groupEntity;
        }
    }

    /// <summary>
    /// 专题实体的扩展类
    /// </summary>
    public static class TopicEntityExtensions
    {
        /// <summary>
        /// 将数据库中的信息转换成EditModel实体
        /// </summary>
        /// <param name="groupEntity"></param>
        /// <returns></returns>
        public static TopicEditModel AsEditModel(this TopicEntity groupEntity)
        {
            return new TopicEditModel
            {
                TopicId = groupEntity.TopicId,
                IsPublic = groupEntity.IsPublic,
                TopicName = groupEntity.TopicName,
                TopicKey = groupEntity.TopicKey,
                Logo = groupEntity.Logo,
                Description = Formatter.FormatMultiLinePlainTextForEdit(groupEntity.Description, true),
                AreaCode = groupEntity.AreaCode,
                JoinWay = groupEntity.JoinWay,
                EnableMemberInvite = groupEntity.EnableMemberInvite,
                CategoryId = groupEntity.Category == null ? 0 : groupEntity.Category.CategoryId,
                Question = groupEntity.Question,
                Answer = groupEntity.Answer
            };
        }
    }
}
