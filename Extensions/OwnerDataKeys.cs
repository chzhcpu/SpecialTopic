﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;

namespace SpecialTopic.Topic
{
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 创建的群组数
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string CreatedTopicCount(this OwnerDataKeys ownerDataKeys)
        {
            return TopicConfig.Instance().ApplicationKey + "-ThreadCount";
        }

        /// <summary>
        /// 加入的群组数
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string JoinedTopicCount(this OwnerDataKeys ownerDataKeys)
        {
            return TopicConfig.Instance().ApplicationKey + "JoinedTopicCount";
        }
       
        /// <summary>
        /// 群组的内容数
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string TopicContentCount(this OwnerDataKeys ownerDataKeys)
        {
            return TopicConfig.Instance().ApplicationKey + "TopicContentCount";
        }
    }
}