using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity.Modules.AttachmentTools;
using SpineEx;

using Object = System.Object;
namespace Spine.Unity
{
	public partial class SkeletonRenderer : MonoBehaviour, ISkeletonComponent, IHasSkeletonDataAsset
	{
		#region 这部分的主要逻辑都是从SkeletonJson里抄的
		/// <summary>
		/// 主要逻辑copy SkeletonJson.ReadAttachment
		/// </summary>
		/// <param name="sdEX"></param>
		/// <param name="atlasArray"></param>
		/// <returns></returns>
		public Skin GenSkinBySkniDataAsset(SkinDataEx sdEX, Atlas[] atlasArray)
		{
#if SPINE_TK2D
			throw new NotImplementedException("GenSkinBySkniDataAsset Not Implemented");
#endif
			float scale = this.skeletonDataAsset.scale;
			var attachmentLoader = new AtlasAttachmentLoader(atlasArray);
			var skin = new Skin(sdEX.SkinName);
			var linkedMeshes = new List<SkeletonJson.LinkedMesh>();
			Attachment attachment = null;
			foreach (var slot in sdEX.AllSlots)
			{
				var slotIndex = skeleton.Data.FindSlotIndex(slot.SlotName);
				foreach (var entry in slot.AllAttachments)
				{
					switch (entry.type)
					{
						case AttachmentType.Region:
							{
								RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, entry.AttachmentName, entry.PathName);
								if (region == null)
								{
									Debug.LogError("RegionAttachment New Fail:" + entry.AttachmentName + "," + entry.PathName);
									continue;
								}
								region.Path = entry.PathName;
								#region Region数据的填充
								CheckRegionData(entry.FloatValues);
								Single w = 32, h = 32;
								foreach (var f in entry.FloatValues)
								{
									switch (f.Key)
									{
										case "x":
											region.x = f.FloatValue * scale;
											break;
										case "y":
											region.y = f.FloatValue * scale;
											break;
										case "scaleX":
											region.scaleX = f.FloatValue;
											break;
										case "scaleY":
											region.scaleY = f.FloatValue;
											break;
										case "rotation":
											region.rotation = f.FloatValue;
											break;
										case "width":
											w = f.FloatValue;
											break;
										case "height":
											h = f.FloatValue;
											break;
									}
								}
								region.width = w * scale;
								region.height = h * scale;
								if (entry.ColorValues != null && entry.ColorValues.Count > 0)
								{
									foreach (var c in entry.ColorValues)
									{
										switch (c.Key)
										{
											case "color":
												region.r = c.ColorValue.r;
												region.g = c.ColorValue.g;
												region.b = c.ColorValue.b;
												region.a = c.ColorValue.a;
												break;
										}
									}
								}
								#endregion
								region.UpdateOffset();
								attachment = region;
							}
							break;
						case AttachmentType.Boundingbox:
							{
								BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, entry.AttachmentName);
								if (box == null)
								{
									Debug.LogError("BoundingBoxAttachment New Fail:" + entry.AttachmentName);
									continue;
								}
								#region Box数据填充
								Int32 vertexCount = 0;
								if (entry.IntValues != null && entry.IntValues.Count > 0)
								{
									foreach (var i in entry.IntValues)
									{
										if (i.Key.Equals("vertexCount"))
										{
											vertexCount = i.IntValue;
											break;
										}
									}
								}
								foreach (var fs in entry.FloatArrayValues)
								{
									if (fs.Key.Equals("vertices"))
									{
										ReadVertices(fs, box, vertexCount << 1, scale);
										break;
									}
								}
								#endregion
								attachment = box;
							}
							break;
						case AttachmentType.Mesh:
						case AttachmentType.Linkedmesh:
							{
								MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, entry.AttachmentName, entry.PathName);
								if (mesh == null)
								{
									Debug.LogError("MeshAttachment New Fail:" + entry.AttachmentName + "," + entry.PathName);
									continue;
								}
								#region Mesh数据填充
								mesh.Path = entry.PathName;
								if (entry.ColorValues != null && entry.ColorValues.Count > 0)
								{
									foreach (var c in entry.ColorValues)
									{
										if (c.Key.Equals("color"))
										{
											mesh.r = c.ColorValue.r;
											mesh.g = c.ColorValue.g;
											mesh.b = c.ColorValue.b;
											mesh.a = c.ColorValue.a;
											break;
										}
									}
								}
								Single w = 0, h = 0;
								foreach (var f in entry.FloatValues)
								{
									switch (f.Key)
									{
										case "width":
											w = f.FloatValue * scale;
											break;
										case "height":
											h = f.FloatValue * scale;
											break;
									}
								}
								mesh.Width = w;
								mesh.Height = h;

								String parentStr = null, skinStr = null;
								Boolean deform = true;
								if (entry.StringValues != null && entry.StringValues.Count > 0)
								{
									foreach (var ss in entry.StringValues)
									{
										switch (ss.Key)
										{
											case "parent":
												parentStr = ss.StringValue;
												break;
											case "skin":
												skinStr = ss.StringValue;
												break;
										}
									}
								}
								if (entry.BoolValues != null && entry.BoolValues.Count > 0)
								{
									foreach (var b in entry.BoolValues)
									{
										if (b.Key.Equals("deform"))
										{
											deform = b.BooleanValue;
											break;
										}
									}
								}
								if (parentStr != null)
								{
									mesh.InheritDeform = deform;
									linkedMeshes.Add(new SkeletonJson.LinkedMesh(mesh, skinStr, slotIndex, parentStr));
								}
								KeyFloatArrayPair kfap_Vs = new KeyFloatArrayPair();
								Single[] uvs = null;
								foreach (var fs in entry.FloatArrayValues)
								{
									switch (fs.Key)
									{
										case "vertices":
											kfap_Vs = fs;
											break;
										case "uvs":
											uvs = fs.FloatArrayValue.ToArray();
											break;
									}
								}
								ReadVertices(kfap_Vs, mesh, uvs.Length, scale);
								Int32[] triangles = null, edges = null;
								if (entry.IntArrayValues != null && entry.IntArrayValues.Count > 0)
								{
									foreach (var i32s in entry.IntArrayValues)
									{
										switch (i32s.Key)
										{
											case "triangles":
												triangles = i32s.IntArrayValue.ToArray();
												break;
											case "edges":
												edges = i32s.IntArrayValue.ToArray();
												break;
										}
									}
								}

								mesh.triangles = triangles;
								mesh.regionUVs = uvs;
								mesh.UpdateUVs();

								Int32 hull = 0;
								if (entry.IntValues != null && entry.IntValues.Count > 0)
								{
									foreach (var i in entry.IntValues)
									{
										if (i.Key.Equals("hull"))
										{
											hull = i.IntValue * 2;
											break;
										}
									}
								}
								if (hull != 0)
								{
									mesh.HullLength = hull;
								}
								if (edges != null)
								{
									mesh.Edges = edges;
								}
								#endregion
								attachment = mesh;
							}
							break;
						case AttachmentType.Path:
							{
								PathAttachment pathAttachment = attachmentLoader.NewPathAttachment(skin, entry.AttachmentName);
								if (pathAttachment == null)
								{
									Debug.LogError("PathAttachment New Fail:" + entry.AttachmentName);
									continue;
								}
								#region Path填充数据
								Boolean closed = false, constantSpeed = false;
								if (entry.BoolValues != null && entry.BoolValues.Count > 0)
								{
									foreach (var b in entry.BoolValues)
									{
										switch (b.Key)
										{
											case "closed":
												closed = b.BooleanValue;
												break;
											case "constantSpeed":
												constantSpeed = b.BooleanValue;
												break;
										}
									}
								}
								pathAttachment.closed = closed;
								pathAttachment.constantSpeed = constantSpeed;

								Int32 vertexCount = 0;
								if (entry.IntValues != null && entry.IntValues.Count > 0)
								{
									foreach (var i in entry.IntValues)
									{
										if (i.Key.Equals("vertexCount"))
										{
											vertexCount = i.IntValue;
											break;
										}
									}
								}
								foreach (var fs in entry.FloatArrayValues)
								{
									switch (fs.Key)
									{
										case "vertices":
											ReadVertices(fs, pathAttachment, vertexCount << 1, scale);
											break;
										case "lengths":
											var count = fs.FloatArrayValue.Count;
											pathAttachment.lengths = new Single[count];
											for (var idx = 0 ;idx < count; idx++)
											{
												pathAttachment.lengths[idx] = fs.FloatArrayValue[idx] * scale;
											}
											break;
									}
								}
								#endregion
								attachment = pathAttachment;
							}
							break;
						case AttachmentType.Point:
							{
								PointAttachment point = attachmentLoader.NewPointAttachment(skin, entry.AttachmentName);
								if (point == null)
								{
									Debug.LogError("PointAttachment New Fail:" + entry.AttachmentName);
									continue;
								}
								#region Point填充数据
								Single x = 0, y = 0, r = 0;
								if (entry.FloatValues != null && entry.FloatValues.Count > 0)
								{
									foreach (var f in entry.FloatValues)
									{
										switch (f.Key)
										{
											case "x":
												x = f.FloatValue * scale;
												break;
											case "y":
												y = f.FloatValue * scale;
												break;
											case "rotation":
												r = f.FloatValue;
												break;
										}
									}
								}
								point.x = x;
								point.y = y;
								point.rotation = r;
								#endregion
								attachment = point;
							}
							break;
						case AttachmentType.Clipping:
							{
								ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, entry.AttachmentName);
								if (clip == null)
								{
									Debug.LogError("ClippingAttachment New Fail:" + entry.AttachmentName);
									continue;
								}
								#region Clipping填充数据
								String end = null;
								if (entry.StringValues != null && entry.StringValues.Count > 0)
								{
									foreach (var s in entry.StringValues)
									{
										if (s.Key.Equals("end"))
										{
											end = s.StringValue;
											break;
										}
									}
								}
								if (end != null)
								{
									SlotData sd = skeleton.Data.FindSlot(end);
									if (sd == null) throw new Exception("Clipping end slot not found: " + end);
									clip.EndSlot = sd;
								}

								Int32 vertexCount = 0;
								if (entry.IntValues != null && entry.IntValues.Count > 0)
								{
									foreach (var i in entry.IntValues)
									{
										if (i.Key.Equals("vertexCount"))
										{
											vertexCount = i.IntValue;
											break;
										}
									}
								}
								foreach (var fs in entry.FloatArrayValues)
								{
									if (fs.Key.Equals("vertices"))
									{
										ReadVertices(fs, clip, vertexCount << 1, scale);
										break;
									}
								}
								#endregion
								attachment = clip;
							}
							break;
						default:
							break;
					}
					skin.SetAttachment( slot.SlotName, entry.AttachmentName, attachment, skeleton);
				}
			}
			// Linked meshes.
			for (int i = 0, n = linkedMeshes.Count; i < n; i++)
			{
				var linkedMesh = linkedMeshes[i];
				Skin sk = linkedMesh.skin == null ? skeleton.Data.defaultSkin : skeleton.Data.FindSkin(linkedMesh.skin);
				if (sk == null) throw new Exception("Slot not found: " + linkedMesh.skin);
				Attachment parent = sk.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
				linkedMesh.mesh.UpdateUVs();
			}
			skeleton.Data.Skins.Add(skin);
			return skin;
		}
		private static void ReadVertices(KeyFloatArrayPair kfap, VertexAttachment attachment, int verticesLength, Single scale)
		{
			attachment.WorldVerticesLength = verticesLength;
			float[] vertices = kfap.FloatArrayValue.ToArray();// GetFloatArray(map, "vertices", 1);
			if (verticesLength == vertices.Length)
			{
				if (scale != 1)
				{
					for (int i = 0; i < vertices.Length; i++)
					{
						vertices[i] *= scale;
					}
				}
				attachment.vertices = vertices;
				return;
			}
			ExposedList<float> weights = new ExposedList<float>(verticesLength * 3 * 3);
			ExposedList<int> bones = new ExposedList<int>(verticesLength * 3);
			for (int i = 0, n = vertices.Length; i < n;)
			{
				int boneCount = (int)vertices[i++];
				bones.Add(boneCount);
				for (int nn = i + boneCount * 4; i < nn; i += 4)
				{
					bones.Add((int)vertices[i]);
					weights.Add(vertices[i + 1] * scale);
					weights.Add(vertices[i + 2] * scale);
					weights.Add(vertices[i + 3]);
				}
			}
			attachment.bones = bones.ToArray();
			attachment.vertices = weights.ToArray();
		}
		#endregion
		private void CheckRegionData(List<KeyFloatPair> fs)
		{
			return ;
		}
		private Attachment ReadAttachment(Dictionary<string, Object> map, Skin skin, int slotIndex, string name, SkeletonData skeletonData)
		{
			//float scale = this.skeletonDataAsset.scale;
			return null;
		}
	}
}