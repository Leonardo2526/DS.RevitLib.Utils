using System;

namespace DS.RevitCollisions
{
    /// <summary>
    /// Current collision staus.
    /// </summary>
    public enum CollisionStatus
    {
        /// <summary>
        /// Requires to resolve.
        /// </summary>
        ToResolve,

        /// <summary>
        /// Currently resolving
        /// </summary>
        Resolving,

        /// <summary>
        /// Already resolved.
        /// </summary>
        Resolved,

        /// <summary>
        /// Solution was found and collisions is awaiting to apply this solution.
        /// </summary>
        AwaitingToApply,

        /// <summary>
        /// Failed to resolve.
        /// </summary>
        Unresolved,

        /// <summary>
        /// Resolving was stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// Collision is not valid
        /// </summary>
        Invalid
    }

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Convert <see cref="CollisionStatus"/> to cyrillic transcription.
        /// </summary>
        /// <param name="collisionStatus"></param>
        /// <returns>
        /// Status in cyrillic transcription.
        /// </returns>
        public static String ToCyrillic(this CollisionStatus collisionStatus)
        {
            string status = null;
            switch (collisionStatus)
            {
                case CollisionStatus.ToResolve:
                    status = "Решить";
                    break;
                case CollisionStatus.Resolving:
                    status = "Решается";
                    break;
                case CollisionStatus.Resolved:
                    status = "Решена";
                    break;
                case CollisionStatus.AwaitingToApply:
                    status = "В ожидании";
                    break;
                case CollisionStatus.Stopped:
                    status = "Остановлено";
                    break;
                case CollisionStatus.Unresolved:
                    status = "Не решена";
                    break;
                case CollisionStatus.Invalid:
                    status = "Не валидна";
                    break;
                default:
                    break;
            }

            return status;
        }

    }

}
