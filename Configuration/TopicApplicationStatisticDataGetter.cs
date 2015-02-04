﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using Spacebuilder.Common;
using Tunynet.Utilities;
using Tunynet.Common;

namespace SpecialTopic.Topic
{
    /// <summary>
    /// 帖吧应用数据Url获取器
    /// </summary>
    public class TopicApplicationStatisticDataGetter : IApplicationStatisticDataGetter
    {
        private TopicService groupService = new TopicService();
        /// <summary>
        /// 获取管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public IEnumerable<ApplicationStatisticData> GetManageableDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> barSectionManageableDatas = groupService.GetManageableDatas(tenantTypeId);
            if (barSectionManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TopicPendingCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TopicPendingCount(), "群组",
                 "群组待审核数", barSectionManageableDatas[ApplicationStatisticDataKeys.Instance().TopicPendingCount()])
                {
                    DescriptionPattern = "{0}个群组待审核",
                    Url = SiteUrls.Instance().ManageGroups(auditStatus: AuditStatus.Pending)
                });
            if (barSectionManageableDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TopicAgainCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TopicAgainCount(), "群组",
                 "群组需再审核数", barSectionManageableDatas[ApplicationStatisticDataKeys.Instance().TopicAgainCount()])
                {
                    DescriptionPattern = "{0}个群组需再审核",
                    Url = SiteUrls.Instance().ManageGroups(auditStatus: AuditStatus.Again)
                });
            return applicationStatisticDatas;
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public IEnumerable<ApplicationStatisticData> GetStatisticDatas(string tenantTypeId = null)
        {
            IList<ApplicationStatisticData> applicationStatisticDatas = new List<ApplicationStatisticData>();
            Dictionary<string, long> barThreadStatisticDatas = groupService.GetStatisticDatas(tenantTypeId);
            if (barThreadStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().TotalCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().TotalCount(), "群组",
                 "群组总数", barThreadStatisticDatas[ApplicationStatisticDataKeys.Instance().TotalCount()])
                {
                    DescriptionPattern = "共{0}个群组",
                    Url = SiteUrls.Instance().ManageGroups()
                });
            if (barThreadStatisticDatas.ContainsKey(ApplicationStatisticDataKeys.Instance().Last24HCount()))
                applicationStatisticDatas.Add(new ApplicationStatisticData(ApplicationStatisticDataKeys.Instance().Last24HCount(), "群组",
                 "群组24小时新增数", barThreadStatisticDatas[ApplicationStatisticDataKeys.Instance().Last24HCount()])
                {
                    DescriptionPattern = "24小时新增{0}个群组",
                    Url = SiteUrls.Instance().ManageGroups()
                });
            return applicationStatisticDatas;
        }
    }
}