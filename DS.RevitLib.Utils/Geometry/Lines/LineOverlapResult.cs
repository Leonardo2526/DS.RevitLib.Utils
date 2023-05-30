namespace DS.RevitLib.Utils.Geometry.Lines
{
    /// <summary>
    ///  An enumerated type listing all overlap relationship types between two bound lines.
    /// </summary>
    public enum LineOverlapResult
    {
        /// <summary>
        /// Intersection point lies on segments of line1 and line2.
        /// </summary>
        SegementOverlap,

        /// <summary>
        /// Intersection point lies on segment of line1 or line2.
        /// </summary>
        SegmentPointOverlap,

        /// <summary>
        /// Intersection point don't lies on segment of line1 or line2.
        /// </summary>
        PointOverlap,

        /// <summary>
        /// Line1 and line2 have no intersections.
        /// </summary>
        None
    }
}
