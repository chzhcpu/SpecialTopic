﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace SpecialTopic.Topic
{

    /// <summary>
    /// 群组动态项
    /// </summary>
    public static class ActivityItemKeysExtension
    {
        /// <summary>
        /// 创建群组动态项
        /// </summary>
        public static string CreateTopic(this ActivityItemKeys activityItemKeys)
        {
            return "CreateTopic";
        }

        /// <summary>
        /// 新成员加入动态项
        /// </summary>
        public static string CreateTopicMember(this ActivityItemKeys activityItemKeys)
        {
            return "CreateTopicMember";
        }
        /// <summary>
        /// 加入群组动态项
        /// </summary>
        public static string JoinTopic(this ActivityItemKeys activityItemKeys)
        {
            return "JoinTopic";
        }
    }
}