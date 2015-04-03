// ------------------------------------------------------------------------
// 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org/>
// 
// ------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HurstSimulator
{
	public partial class FormMain : Form
	{
		private readonly Random _random = new Random();
		private List<PointF>[] _points;

		public FormMain()
		{
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
			thread.IsBackground = true;
			thread.Start();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			List<PointF>[] points = _points;
			if (points != null)
			{
				e.Graphics.DrawLines(System.Drawing.Pens.Red, points[0].ToArray());
				e.Graphics.DrawLines(System.Drawing.Pens.Lime, points[1].ToArray());
			}
		}

		private void Run()
		{
			while (true)
			{
				List<PointF> pointsA = Curve(0.0, 0.0, 1.0, 0.5, 0.04, 1.0);
				List<PointF> pointsB = Curve(0.0, 0.0, 1.0, -0.5, 0.01, 0.5);

				double x_min = pointsA[0].X;
				double x_max = pointsA[0].X;
				double y_min = pointsA[0].Y;
				double y_max = pointsA[0].Y;
				for (int i = 1; i < pointsA.Count; i++)
				{
					if (pointsA[i].X < x_min)
						x_min = pointsA[i].X;
					if (pointsA[i].X > x_max)
						x_max = pointsA[i].X;
					if (pointsA[i].Y < y_min)
						y_min = pointsA[i].Y;
					if (pointsA[i].Y > y_max)
						y_max = pointsA[i].Y;
				}
				for (int i = 0; i < pointsB.Count; i++)
				{
					if (pointsB[i].X < x_min)
						x_min = pointsB[i].X;
					if (pointsB[i].X > x_max)
						x_max = pointsB[i].X;
					if (pointsB[i].Y < y_min)
						y_min = pointsB[i].Y;
					if (pointsB[i].Y > y_max)
						y_max = pointsB[i].Y;
				}
				x_min -= (x_max - x_min) / 20.0;
				x_max += (x_max - x_min) / 20.0;
				y_min -= (y_max - y_min) / 20.0;
				y_max += (y_max - y_min) / 20.0;
				for (int i = 0; i < pointsA.Count; i++)
				{
					double x = (pointsA[i].X - x_min) / (x_max - x_min) * ClientSize.Width;
					double y = (pointsA[i].Y - y_min) / (y_max - y_min) * ClientSize.Height;
					pointsA[i] = new PointF((float)x, (float)(ClientSize.Height - y));
				}
				for (int i = 0; i < pointsB.Count; i++)
				{
					double x = (pointsB[i].X - x_min) / (x_max - x_min) * ClientSize.Width;
					double y = (pointsB[i].Y - y_min) / (y_max - y_min) * ClientSize.Height;
					pointsB[i] = new PointF((float)x, (float)(ClientSize.Height - y));
				}

				_points = new List<PointF>[] { pointsA, pointsB };

				Invalidate();

				System.Threading.Thread.Sleep(1000);
			}
		}

		private List<PointF> Curve(double x0, double y0, double x1, double y1, double var, double H)
		{
			List<PointF> points = new List<PointF>();
			points.Add(new PointF((float)x0, (float)y0));
			double s = Math.Pow(2, 2 * H);
			Curve(x0, y0, x1, y1, var, s, points);
			return points;
		}

		private void Curve(double x0, double y0, double x1, double y1, double var, double s, List<PointF> points)
		{
			if (x1 - x0 < .01) // stop if interval is sufficiently small
			{
				points.Add(new PointF((float)x1, (float)y1));
			}
			else
			{
				double xm = (x0 + x1) / 2;
				double ym = (y0 + y1) / 2;
				ym += gaussian(0, Math.Sqrt(var)); // Gaussian noise
				Curve(x0, y0, xm, ym, var / s, s, points);
				Curve(xm, ym, x1, y1, var / s, s, points);
			}
		}

		private double uniform(double a, double b)
		{
			return a + _random.NextDouble() * (b - a);
		}

		private double gaussian()
		{
			// use the polar form of the Box-Muller transform
			double r, x, y;
			do
			{
				x = uniform(-1.0, 1.0);
				y = uniform(-1.0, 1.0);
				r = x * x + y * y;
			} while (r >= 1 || r == 0);
			return x * Math.Sqrt(-2 * Math.Log(r) / r);

			// Remark:  y * Math.sqrt(-2 * Math.log(r) / r)
			// is an independent random gaussian
		}

		private double gaussian(double mean, double stddev)
		{
			return mean + stddev * gaussian();
		}

	}
}