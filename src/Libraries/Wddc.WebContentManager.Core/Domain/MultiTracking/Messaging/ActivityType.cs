using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    public enum ActivityType
    {
        CreatedComment, EditedComment, AssignedMessage, ClosedMessage, RanAction, ReopenedIssue
    }
}
