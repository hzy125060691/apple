using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIRadarChartGraphic : MaskableGraphic
{
	#region 抄UGUI的代码
	[FormerlySerializedAs("m_Tex"), SerializeField]
	private Texture m_Texture;
	public override Texture mainTexture
	{
		get
		{
			Texture result;
			if (this.m_Texture == null)
			{
				if (this.material != null && this.material.mainTexture != null)
				{
					result = this.material.mainTexture;
				}
				else
				{
					result = Graphic.s_WhiteTexture;
				}
			}
			else
			{
				result = this.m_Texture;
			}
			return result;
		}
	}

	protected static Material s_ETC1DefaultUI = null;
	public static Material defaultETC1GraphicMaterial
	{
		get
		{
			if (s_ETC1DefaultUI == null)
			{
				s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
			}
			return s_ETC1DefaultUI;
		}
	}
	public override Material material
	{
		get
		{
			Material result;
			if ((UnityEngine.Object)base.m_Material != (UnityEngine.Object)null)
			{
				result = this.m_Material;
			}
			else
			{
				result = this.defaultMaterial;
			}
			return result;
		}
		set
		{
			base.material = value;
		}
	}
	#endregion
	#region 抄jack.sydorenko, firagon的代码 
	/// Sourced from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
	/// Updated/Refactored from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/#post-2528050

	private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

	//private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;
	//private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0, 1);
	private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
	private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
	//private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1, 0);
	//private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1, 1);

	//private static readonly Vector2[] startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };
	private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
	//private static readonly Vector2[] endUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };

	private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, Color color)
	{
		var uvs = middleUvs;

		Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2;
		var v1 = start - offset;
		var v2 = start + offset;
		var v3 = end + offset;
		var v4 = end - offset;
		return SetVbo(new[] { v1, v2, v3, v4 }, uvs, color);
	}
	protected static UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color color)
	{
		UIVertex[] vbo = new UIVertex[4];
		for (int i = 0; i < vertices.Length; i++)
		{
			var vert = UIVertex.simpleVert;
			vert.color = color;
			vert.position = vertices[i];
			vert.uv0 = uvs[i];
			vbo[i] = vert;
		}
		return vbo;
	}
	#endregion

	#region 自己定义的数据
	[Serializable]
	public struct MinMaxValue
	{
		[Range(0, 1f)]
		[SerializeField]
		public Single Min;
		[Range(0, 1f)]
		[SerializeField]
		public Single Max;
	}
	#endregion
	[SerializeField]
	public Boolean LineMode;
	[SerializeField]
	public Single LineThickness = 2;
	[SerializeField]
	public MinMaxValue[] AllMinMaxValues;
	[SerializeField]
	public Single Angle;
	[NonSerialized]
	private Int32 AllCount;
	[NonSerialized]
	private Vector3[] vertexes;//内切圆上的顶点位置，未乘以对应的value
	[NonSerialized]
	private List<Vector3> SplitVertexes = new List<Vector3>();//分割角度后新出现的点
	[NonSerialized]
	private List<Vector3> RealSplitVertexes = new List<Vector3>();//分割角度后新出现的点乘以了value
	[NonSerialized]
	private List<Vector3> RealMinSplitVertexes = new List<Vector3>();//分割角度后新出现的点乘以对应的min value(这是最小值)
	[NonSerialized]
	private Vector3[] realVertexes;//内切圆上的顶点乘以对应的max value(这是最大值)
	[NonSerialized]
	private Vector3[] realMinVertexes;//内切圆上的顶点乘以对应的min value(这是最小值)
	[NonSerialized]
	private Vector2[] uvs;
	[NonSerialized]
	private Color[] colors;
	[NonSerialized]
	private List<UIVertex[]>  Segments = new List<UIVertex[]>();
	public void InitCount(Single[] pers, Single angle = 0, Color[] colors = null)
	{
		var tmpMM = new MinMaxValue[pers.Length];
		for (var i = 0; i < pers.Length; i++)
		{
			tmpMM[i].Min = 0;
			tmpMM[i].Max = pers[i];
		}
		InitCount(tmpMM, angle, colors);
	}
	public void InitCount(MinMaxValue[] pers, Single angle = 0, Color[] colors = null)
	{
		if (pers.Length < 3)
		{
			return;
		}
		if (angle > 0)
		{
			Angle = angle;
		}
		AllCount = pers.Length;

		if (vertexes == null || vertexes.Length < AllCount)
		{
			vertexes = new Vector3[AllCount];
		}
		if (realVertexes == null || realVertexes.Length < AllCount)
		{
			realVertexes = new Vector3[AllCount];
		}
		if (realMinVertexes == null || realMinVertexes.Length < AllCount)
		{
			realMinVertexes = new Vector3[AllCount];
		}

		if (uvs == null || uvs.Length < AllCount)
		{
			uvs = new Vector2[AllCount];
			//全是0，没赋值，暂时这么放着
		}
		this.colors = colors;
		AllMinMaxValues = pers;
		SetAllDirty();
	}
	private void SplitQuad()
	{
		var allAngles = AllCount * Angle;
		SplitVertexes.Clear();
		//单独的角度*数量大于等于360度就不用分割了，就是一整块
		if (allAngles < 360 && allAngles > 0)
		{
			var lastVertex = vertexes[AllCount - 1];
			Vector3 nextVertex, vertex;
			var per = Angle * AllCount / (2 * 360);
			var oneMinusper = 1 - per;
			for (var i = 0; i < AllCount - 1; i++)
			{
				vertex = vertexes[i];
				nextVertex = vertexes[i + 1];
				//顶点左右两边按度数得到的两个点，在两个定点连线上
				SplitVertexes.Add(oneMinusper * vertex + lastVertex * per);
				SplitVertexes.Add(oneMinusper * vertex + nextVertex * per);
				lastVertex = vertex;
			}
			vertex = vertexes[AllCount - 1];
			nextVertex = vertexes[0];
			SplitVertexes.Add(oneMinusper * vertex + lastVertex * per);
			SplitVertexes.Add(oneMinusper * vertex + nextVertex * per);

		}
	}
	private Boolean RefreshRealVertex()
	{
		RealSplitVertexes.Clear();
		RealMinSplitVertexes.Clear();
		var bSplit = SplitVertexes.Count > 0;
		var size = rectTransform.rect;
		var bMinZero = true;//是否所有的最小值点都是0，这样可以把图形合并一下
		for (var i = 0; i < AllCount; i++)
		{
			//设置对应的真正顶点的最大值最小值，方便计算是六边形还是四边形
			realVertexes[i] = vertexes[i] * AllMinMaxValues[i].Max * new Vector2(size.width, size.height);
			realMinVertexes[i] = vertexes[i] * AllMinMaxValues[i].Min * new Vector2(size.width, size.height);
			if (AllMinMaxValues[i].Min > 0)
			{
				bMinZero = false;
			}
		}

		if(bSplit)
		{
			for (var i = 0; i < AllCount; i++)
			{
				RealSplitVertexes.Add(SplitVertexes[2 * i] * AllMinMaxValues[i].Max * new Vector2(size.width, size.height));
				RealSplitVertexes.Add(SplitVertexes[2 * i + 1] * AllMinMaxValues[i].Max * new Vector2(size.width, size.height));

				RealMinSplitVertexes.Add(SplitVertexes[2 * i] * AllMinMaxValues[i].Min * new Vector2(size.width, size.height));
				RealMinSplitVertexes.Add(SplitVertexes[2 * i + 1] * AllMinMaxValues[i].Min * new Vector2(size.width, size.height));
			}
		}
		if(LineMode)
		{
			UIVertex[] tmp = null;
			Segments.Clear();
			//有的时候两个连接着线段中间需要做个过度的处理，会稍微修正一下坐标或者加入一个新的线段，
			//那么就需要区分一下那些线段是连接的，那些不是连接着的
			//所以在每个分支里都有JoinTwoSegments的操作
			if (bSplit)
			{
				if(bMinZero)
				{
					//这个还是四边形
					for (var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						var line0 = CreateLineSegment(Vector3.zero, RealSplitVertexes[i * 2], c);
						var line1 = CreateLineSegment(RealSplitVertexes[i * 2], realVertexes[i], c);
						var line2 = CreateLineSegment(realVertexes[i], RealSplitVertexes[i * 2 + 1], c);
						var line3 = CreateLineSegment(RealSplitVertexes[i * 2 + 1], Vector3.zero, c);

						//虽然已经有了四条线了，但是线与线之间衔接太生硬，需要处理一下转角
						if(JoinTwoSegments(ref line0, ref line1, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line1, ref line2, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line2, ref line3, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line3, ref line0, out tmp))
						{
							Segments.Add(tmp);
						}
						Segments.Add(line0);
						Segments.Add(line1);
						Segments.Add(line2);
						Segments.Add(line3);
					}
				}
				else
				{
					//这个变成六边形了，还是凹的
					for (var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						var line0 = CreateLineSegment(realMinVertexes[i], RealMinSplitVertexes[i * 2], c);
						var line1 = CreateLineSegment(RealMinSplitVertexes[i * 2], RealSplitVertexes[i * 2], c);
						var line2 = CreateLineSegment(RealSplitVertexes[i * 2], realVertexes[i], c);
						var line3 = CreateLineSegment(realVertexes[i], RealSplitVertexes[i * 2 + 1], c);
						var line4 = CreateLineSegment(RealSplitVertexes[i * 2 + 1], RealMinSplitVertexes[i * 2 + 1], c);
						var line5 = CreateLineSegment(RealMinSplitVertexes[i * 2 + 1], realMinVertexes[i], c);
						if (JoinTwoSegments(ref line0, ref line1, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line1, ref line2, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line2, ref line3, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line3, ref line4, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line4, ref line5, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line5, ref line0, out tmp))
						{
							Segments.Add(tmp);
						}
						Segments.Add(line0);
						Segments.Add(line1);
						Segments.Add(line2);
						Segments.Add(line3);
						Segments.Add(line4);
						Segments.Add(line5);
					}
				}
			}
			else
			{
				if(bMinZero)
				{
					//这是一个N边型,这个点的颜色很尴尬，就这么放着吧
					var c = colors == null ? this.color : this.color * colors[0];
					var lastLine = CreateLineSegment(realVertexes[AllCount - 1], realVertexes[0], c);
					var lastIdx = AllCount - 1;
					for (var i = 0; i < AllCount; i++)
					{
						Segments.Add(CreateLineSegment(realVertexes[lastIdx], realVertexes[i], colors == null ? this.color : this.color * colors[i]));
						lastIdx = i;
					}
					var count = Segments.Count;
					lastIdx = count - 1;
					for (var i = 0; i < count;i++)
					{
						var line1 = Segments[lastIdx];
						var line2 = Segments[i];
						if (JoinTwoSegments(ref line1, ref line2, out tmp))
						{
							Segments.Add(tmp);
						}
						lastIdx = i;
					}
				}
				else
				{
					//这是一个类似环形的东西，长得像空心的多边形，但是把它拆成一堆四边形的集合
					//是line模式，这个中空的奇怪图形会有很多重合部分，所以下边的做操很多是没用的，但是还是统一做一次
					var lastIndex = AllCount - 1;
					for (var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						var line0 = CreateLineSegment(realMinVertexes[i], realVertexes[i], c);
						var line1 = CreateLineSegment(realVertexes[i], realVertexes[lastIndex], c);
						var line2 = CreateLineSegment(realVertexes[lastIndex], realMinVertexes[lastIndex], c);
						var line3 = CreateLineSegment(realMinVertexes[lastIndex], realMinVertexes[i], c);

						if(JoinTwoSegments(ref line0, ref line1, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line1, ref line2, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line2, ref line3, out tmp))
						{
							Segments.Add(tmp);
						}
						if (JoinTwoSegments(ref line3, ref line0, out tmp))
						{
							Segments.Add(tmp);
						}
						Segments.Add(line0);
						Segments.Add(line1);
						Segments.Add(line2);
						Segments.Add(line3);
						lastIndex = i;
					}
				}
			}


		}
		return bMinZero;
	}
	/// <summary>
	/// 处理两个线段，把根据两个线段的夹角，填充修改原线段并且增加一个新线段使之平滑连在一起
	/// line1的【2】 【3】中点与line2的【0】 【1】中点必须重合
	/// </summary>
	/// <param name="line1">这是一个四边形</param>
	/// <param name="line2">这是一个四边形</param>
	/// <param name="join">这是把两个四边形中间的缝隙抹掉后的新出现的四边形</param>
	/// <returns></returns>
	private Boolean JoinTwoSegments(ref UIVertex[] line1, ref UIVertex[] line2, out UIVertex[] join)
	{
		var vec1 = line1[1].position - line1[2].position;   //线段方向（start指向end方向）右侧边缘的向量反向
		var vec2 = line2[2].position - line2[1].position;   //线段方向（start指向end方向）右侧边缘的向量

		//两个向量的夹角（0到pi），无符号
		var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;
		// 正数是顺时针方向的角度↑↑↑↑↑，负数逆时针的↑↑↑↑↑↑↑↑
		// Positive sign means the line is turning in a 'clockwise' direction
		var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

		// Calculate the miter point
		var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
		//AB这两个点是 两个四边形中的  （end-start）向量绕交点旋转一半角度与 四边形line1两条边（或延长线）的交点
		var miterPointA = line1[2].position - vec1.normalized * miterDistance * sign;
		var miterPointB = line1[3].position + vec1.normalized * miterDistance * sign;

		if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
		{
			if (sign < 0)
			{
				line1[2].position = miterPointA;
				line2[1].position = miterPointA;
			}
			else
			{
				line1[3].position = miterPointB;
				line2[0].position = miterPointB;
			}
			join = new UIVertex[] { line1[2], line1[3], line2[0], line2[1] };
			return true;
		}
		else
		{
			join = null;
		}

		return false;
	}
	private void SetVertex(Int32 idx)
	{
		var hh = 0.5f;
		var hw = hh;
		//var per = (Single)idx / AllCount;
		{
			var tarRad =  idx * Mathf.PI * 2 / AllCount;
			tarRad = tarRad > Mathf.PI ? tarRad - Mathf.PI * 2 : tarRad;
			var tarSin = Mathf.Sin(tarRad);
			var tarCos = Mathf.Cos(tarRad);
			//var xFlag = tarRad >= 0 ? 1 : -1;
			//var yFlag = Mathf.Abs(tarRad) <= Mathf.PI/2 ? 1 : -1;
			vertexes[idx] = new Vector3(tarSin * hh,  hw * tarCos, 0);
		}
	}
	protected sealed override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (AllCount < 3 || vertexes == null)
		{
			Debug.LogError("OnPopulateMesh:" + AllCount + "," + (vertexes == null));
			return;
		}
		for (var i = 0; i < AllCount; i++)
		{
			SetVertex(i);
		}
		SplitQuad();
		var isZeroMin = RefreshRealVertex();
		//Debug.Log("OnPopulateMesh");
		if(!LineMode)
		{
			//非line模式
			var bSplit = RealSplitVertexes.Count > 0;
			if(isZeroMin)
			{
				//如果0点都重合在中心
				if(bSplit)
				{
					//这是多个四边形
					for(var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						vh.AddVert(realVertexes[i], c, uvs[i]);
						vh.AddVert(RealSplitVertexes[2 * i], c, uvs[i]);
						vh.AddVert(RealSplitVertexes[2 * i + 1], c, uvs[i]);
					}
					//把零点放在最后,这个点的颜色很尴尬，就这么放着吧
					vh.AddVert(Vector3.zero, colors == null ? this.color : this.color * colors[0], new Vector2(0.5f, 0.5f));
					var centerIdx = vh.currentVertCount - 1;
					var threeCount = AllCount * 3;
					//2n个三角形形
					for (var i = 0; i < AllCount; i++)
					{
						var idx = i * 3;
						vh.AddTriangle(centerIdx, idx/* % threeCount*/, (idx + 1) % threeCount);
						vh.AddTriangle(centerIdx, idx/* % threeCount*/, (idx + 2) % threeCount);
					}
				}
				else
				{
					//这是1个N边形
					for (var i = 0; i < AllCount; i++)
					{
						vh.AddVert(realVertexes[i], colors == null ? this.color : this.color * colors[i], uvs[i]);
					}
					//把零点放在最后,这个点的颜色很尴尬，就这么放着吧
					vh.AddVert(Vector3.zero, colors == null ? this.color : this.color * colors[0], new Vector2(0.5f, 0.5f));
					var centerIdx = vh.currentVertCount - 1;
					//var threeCount = AllCount * 3;
					//n个三角形形
					for (var i = 0; i < AllCount; i++)
					{
						vh.AddTriangle(centerIdx, i /*% AllCount*/, (i + 1) % AllCount);
					}
				}
			}
			else
			{
				//最小值点都不重合
				if(bSplit)
				{
					//这里就是n个凹六边形
					for (var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						vh.AddVert(realMinVertexes[i], c, new Vector2(0.5f, 0.5f));
						vh.AddVert(RealMinSplitVertexes[2 * i], c, uvs[i]);
						vh.AddVert(RealSplitVertexes[2 * i], c, uvs[i]);
						vh.AddVert(realVertexes[i], c, uvs[i]);
						vh.AddVert(RealSplitVertexes[2 * i + 1], c, uvs[i]);
						vh.AddVert(RealMinSplitVertexes[2 * i + 1], c, uvs[i]);
					}
					//以单独凹六边形的中心线最小值为中心，画4个三角形
					for (var i = 0; i < AllCount; i++)
					{
						var idx = i * 6;
						vh.AddTriangle(idx, idx + 1, idx + 2);
						vh.AddTriangle(idx, idx + 2, idx + 3);
						vh.AddTriangle(idx, idx + 3, idx + 4);
						vh.AddTriangle(idx, idx + 4, idx + 5);
					}
				}
				else
				{
					//这里就是n个四边形
					for (var i = 0; i < AllCount; i++)
					{
						var c = colors == null ? this.color : this.color * colors[i];
						vh.AddVert(realMinVertexes[i], c, new Vector2(0.5f, 0.5f));
						vh.AddVert(realVertexes[i], c, uvs[i]);
					}
					//n个四边形拆成2n个三角形
					var lastIdx1 = vh.currentVertCount - 2;
					var lastIdx2 = vh.currentVertCount - 1;
					for (var i = 0; i < AllCount; i++)
					{
						var idx = i * 2;
						vh.AddTriangle(idx, idx+1, lastIdx2);
						vh.AddTriangle(idx, lastIdx2, lastIdx1);
						lastIdx1 = idx ;
						lastIdx2 = idx + 1;
					}
				}
			}

		}
		else
		{
			//if(isZeroMin)
			{
				foreach (var seg in Segments)
				{
					vh.AddUIVertexQuad(seg);
				}
			}

		}
	}
#if UNITY_EDITOR
	private void Refresh()
	{
		if (AllMinMaxValues == null || AllMinMaxValues.Length == 0)
		{
			AllMinMaxValues = new MinMaxValue[3];
		}
		if (RandomValue)
		{
			for (var i = 0; i < AllMinMaxValues.Length; i++)
			{
				AllMinMaxValues[i].Min = UnityEngine.Random.Range(0, 0.2f);
				AllMinMaxValues[i].Max = UnityEngine.Random.Range(0, 1f);
			}
		}
		InitCount(AllMinMaxValues);
	}
	public Boolean RandomValue = false;
	[ContextMenu("Refresh count")]
	private void EditorRereshCount()
	{
		Refresh();
		SetAllDirty();
	}

	protected sealed override void OnValidate()
	{
		//Debug.Log("OnValidate");
		EditorRereshCount();
		base.OnValidate();
	}
	protected sealed override void UpdateGeometry()
	{
		if (AllCount < 3 || vertexes == null)
		{
			Debug.LogWarning("OnPopulateMesh:" + AllCount + "," + (vertexes == null));
			return;
		}
		for (var i = 0; i < AllCount; i++)
		{
			SetVertex(i);
		}
		SplitQuad();
		RefreshRealVertex();
		//Debug.Log("UpdateGeometry");
		base.UpdateGeometry();
	}
#endif
}
