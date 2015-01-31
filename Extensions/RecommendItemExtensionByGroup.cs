﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace SpecialTopic.Topic
{
    /// <summary>
    /// 推荐项类型扩展类
    /// </summary>
    public static class RecommendItemExtensionByGroup
    {
        /// <summary>
        /// 
        /// </summary>
        public static TopicEntity GetGroup(this RecommendItem item)
        {
            return new TopicService().Get(item.ItemId);
        }
    }
}