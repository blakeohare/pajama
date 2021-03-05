using System.Collections.Generic;
using Pajama.Node;

namespace Pajama
{
	internal abstract class PyGameStandins
	{
		private List<string> imagesToPreload;

		public PyGameStandins(List<string> imagesToPreload)
		{
			this.imagesToPreload = imagesToPreload;
		}

		public bool MaybeSerializeStandin(string indent, List<string> buffer, Class cls, ClassMember member)
		{
			string key = cls.FullName + "|" + (member == null ? "_constructor_" : member.Name);
			switch (key)
			{
				case "PJ|print": this.Serialize_Pj_print(indent, buffer); return true;

				case "PJDraw|ellipse": this.Serialize_PjDraw_ellipse(indent, buffer); return true;
				case "PJDraw|rectangle": this.Serialize_PjDraw_rectangle(indent, buffer); return true;

				case "PJEvent|convertKeyCode": this.Serialize_PjEvent_convertKeyCode(indent, buffer); return true;

				case "PJGame|_constructor_": this.Serialize_PjGame_init(indent, buffer); return true;
				case "PJGame|start": this.Serialize_PjGame_start(indent, buffer, this.imagesToPreload); return true;
				case "PJGame|tick": this.Serialize_PjGame_tick(indent, buffer); return true;

				case "PJImage|_constructor_": this.Serialize_PjImage_init(indent, buffer); return true;
				case "PJImage|blit": this.Serialize_PjImage_blit(indent, buffer); return true;
				case "PJImage|fill": this.Serialize_PjImage_fill(indent, buffer); return true;
				case "PJImage|loadFromFile": this.Serialize_PjImage_loadFromFile(indent, buffer); return true;
				case "PJImage|createBlank": this.Serialize_PjImage_createBlank(indent, buffer); return true;

				case "Time|currentFloat": this.Serialize_Time_currentFloat(indent, buffer); return true;
				case "Time|currentInt": this.Serialize_Time_currentInt(indent, buffer); return true;

				default: return false;
			}
		}

		protected void TheseLines(string indent, List<string> buffer, params string[] lines)
		{
			foreach (string line in lines)
			{
				buffer.Add(indent + line);
			}
		}

		protected abstract void Serialize_Pj_print(string indent, List<string> buffer);

		protected abstract void Serialize_PjDraw_rectangle(string indent, List<string> buffer);
		protected abstract void Serialize_PjDraw_ellipse(string indent, List<string> buffer);
		protected abstract void Serialize_PjDraw_line(string indent, List<string> buffer);
		protected abstract void Serialize_PjDraw_text(string indent, List<string> buffer);

		protected abstract void Serialize_PjEvent_convertKeyCode(string indent, List<string> buffer);

		protected abstract void Serialize_PjImage_init(string indent, List<string> buffer);
		protected abstract void Serialize_PjImage_blit(string indent, List<string> buffer);
		protected abstract void Serialize_PjImage_fill(string indent, List<string> buffer);
		protected abstract void Serialize_PjImage_loadFromFile(string indent, List<string> buffer);
		protected abstract void Serialize_PjImage_createBlank(string indent, List<string> buffer);

		protected abstract void Serialize_PjGame_init(string indent, List<string> buffer);
		protected abstract void Serialize_PjGame_start(string indent, List<string> buffer, List<string> imagesToPreload);
		protected abstract void Serialize_PjGame_tick(string indent, List<string> buffer);

		protected abstract void Serialize_Time_currentFloat(string indent, List<string> buffer);
		protected abstract void Serialize_Time_currentInt(string indent, List<string> buffer);
	}
}