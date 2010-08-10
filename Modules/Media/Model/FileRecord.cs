namespace Triton.Media.Model
{
	public class FileRecord
	{
		///Pending further investigation this will be much too inefficient for regular use.
		//private FileInfo fileInfo = null;
		//public FileInfo FileInfo
		//{
		//    get 
		//    {
		//        if (this.fileInfo == null) {
		//            if (this.Path != null && this.Name != null) {
		//                string fullPath = string.Format("{0}{1}", this.Path, this.Name);  
		//                this.fileInfo = new FileInfo(this.Path + this.Name);	
		//            }
		//        }
		//        return fileInfo;
		//    }
		//}
		public virtual string Path { get; set; }

		public virtual string Name { get; set; }
	}
}