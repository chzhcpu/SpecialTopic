//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

namespace SpecialTopic.Topic
{
    /// <summary>
    /// 通过群组数据仓储实现查询
    /// </summary>
    public class DefaultTopicIdToTopicKeyDictionary : TopicIdToTopicKeyDictionary
    {
        private ITopicRepository groupRepository;
        /// <summary>
        /// 构造器
        /// </summary>
        public DefaultTopicIdToTopicKeyDictionary()
            : this(new TopicRepository())
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        public DefaultTopicIdToTopicKeyDictionary(ITopicRepository groupRepository)
        {
            this.groupRepository = groupRepository;
        }

        /// <summary>
        /// 根据群组Id获取群组Key
        /// </summary>
        /// <returns>
        /// 群组Id
        /// </returns>
        protected override string GetGroupKeyByGroupId(long groupId)
        {
            TopicEntity group = groupRepository.Get(groupId);
            if (group != null)
                return group.TopicKey;
            return null;
        }

        /// <summary>
        /// 根据群组Key获取群组Id
        /// </summary>
        /// <param name="groupKey">群组Key</param>
        /// <returns>
        /// 群组Id
        /// </returns>
        protected override long GetGroupIdByGroupKey(string groupKey)
        {
            return groupRepository.GetTopicIdByTopicKey(groupKey);
        }
    }
}
