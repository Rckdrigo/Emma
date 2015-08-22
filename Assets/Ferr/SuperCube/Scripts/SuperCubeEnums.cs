namespace Ferr {
	/// <summary>
	/// Used to determine the UV generation algorithm for Ferr SuperCube meshes
	/// </summary>
	public enum UVType {
		/// <summary>
		/// Generates UVs from the vertex's position in world space
		/// </summary>
		WorldCoordinates,
		/// <summary>
		/// Generates UVs from the vertex's position in model space
		/// </summary>
		LocalCoordinates,
		/// <summary>
		/// Generates UVS that are useful for walls, 0-1 on the Y axis, and repeating at an aspect ratio correct distance on the X axis
		/// </summary>
		WallSlide,
		/// <summary>
		/// Generates UVs 0-1 on both X and Y axes
		/// </summary>
		Unit
	}

	public enum PivotType {
		None    = 0,
		Front   = 1,
		Back    = 2,
		Left    = 4,
		Right   = 8,
		Top     = 16,
		Bottom  = 32,
		XCenter = 4  | 8,
		YCenter = 16 | 32,
		ZCenter = 1  | 2,
		Center  = 1  | 2 | 4 | 8 | 16 | 32,
		All     = 1  | 2 | 4 | 8 | 16 | 32
	}
}
