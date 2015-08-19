using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SourceControlSync.DataVSO
{
    [DataContract]
    public class VSOCodePushed
    {
        [DataMember(Name = "subscriptionId")]
        public Guid SubscriptionId { get; set; }
        [DataMember(Name = "notificationId")]
        public long NotificationId { get; set; }
        [DataMember(Name = "id")]
        public Guid Id { get; set; }
        [DataMember(Name = "eventType")]
        public string EventType { get; set; }
        [DataMember(Name = "publisherId")]
        public string PublisherId { get; set; }
        [DataMember(Name = "resource")]
        public Microsoft.TeamFoundation.SourceControl.WebApi.GitPush Resource { get; set; }
        [DataMember(Name = "createdDate")]
        public DateTime CreatedDate { get; set; }

        public Push ToSync()
        {
            return Resource.ToSync();
        }
    }
}
