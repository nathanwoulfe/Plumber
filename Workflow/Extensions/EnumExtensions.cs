using System.Linq;
using Workflow.Models;

namespace Workflow.Extensions
{
    internal static class EnumExtensions
    {
        /// <summary>
        /// Is the task status included in the provided arguments set
        /// </summary>
        /// <param name="status"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static bool In(this TaskStatus? status, params object[] set)
        {
            return set.Contains(status);
        }

        /// <summary>
        /// Is the task status NOT included in the provided arguments set
        /// </summary>
        /// <param name="status"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static bool NotIn(this TaskStatus? status, params object[] set)
        {
            return !set.Contains(status);
        }

        /// <summary>
        /// Is the workflow status included in the provided arguments set
        /// </summary>
        /// <param name="status"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static bool In(this WorkflowStatus status, params object[] set)
        {
            return set.Contains(status);
        }

        /// <summary>
        /// Is the workflow status NOT included in the provided arguments set
        /// </summary>
        /// <param name="status"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        public static bool NotIn(this WorkflowStatus status, params object[] set)
        {
            return !set.Contains(status);
        }
    }
}
