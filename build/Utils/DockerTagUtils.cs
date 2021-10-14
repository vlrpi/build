namespace Utils
{
	internal static class DockerTagUtils
	{
		public static string WithImage(this string tag, string image) => tag.Replace("{IMAGE}", image);
	}
}