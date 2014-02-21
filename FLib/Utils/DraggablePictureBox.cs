using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace FLib
{
    public partial class DraggablePictureBox : UserControl
    {
        public Bitmap Bmp;
        public float scaleX = 1;
        public float scaleY = 1;
        Matrix transform = new Matrix();
        bool dragging = false;
        Point prevMousePos;

        public DraggablePictureBox()
        {
            InitializeComponent();

            MouseWheel += canvas_MouseWheel;
        }

        //----------------------------------------------------------
        // 座標変換
        //

        public Matrix Zoom(float x, float y)
        {
            transform.Scale(x, y, MatrixOrder.Append);
            scaleX *= x;
            scaleY *= y;
            return transform;
        }

        public Matrix Translate(float x, float y)
        {
            transform.Translate(x, y, MatrixOrder.Append);
            return transform;
        }

        // キャンバス（ピクチャボックス）の座標系 -> ワールド座標系
        public Point PointToCanvas(Point pt)
        {
            var pts = new[] { pt };
            transform.TransformPoints(pts);
            return pts[0];
        }

        public Point PointToWorld(Point pt)
        {
            if (transform.IsInvertible == false) return Point.Empty;

            var pts = new[] { pt };

            Matrix invTransform = new Matrix();
            invTransform.Multiply(transform);
            invTransform.Invert();
            invTransform.TransformPoints(pts);

            return pts[0];
        }

        public Rectangle RectangleToCanvas(Rectangle rect)
        {
            Point leftTop = PointToCanvas(rect.Location);
            Point rightBottom = PointToCanvas(new Point(rect.Right, rect.Bottom));
            return new Rectangle(leftTop.X, leftTop.Y, rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y);
        }

        public Rectangle RectangleToWorld(Rectangle rect)
        {
            Point leftTop = PointToWorld(rect.Location);
            Point rightBottom = PointToWorld(new Point(rect.Right, rect.Bottom));
            return new Rectangle(leftTop.X, leftTop.Y, rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y);
        }

        public void Draw()
        {
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (Bmp != null)
            {
                e.Graphics.Transform = transform;
                e.Graphics.DrawImage(Bmp, 0, 0, Bmp.Width, Bmp.Height);
            }
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            prevMousePos = e.Location;
            dragging = true;
            canvas.Invalidate();
        }

        public void canvas_MouseMove(object sender, MouseEventArgs e)
        {

        }

        public void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            prevMousePos = e.Location;
            canvas.Invalidate();
        }

        public void canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            Point pt = canvas.PointToClient(Cursor.Position);
            if (0 <= pt.X && pt.X <= canvas.Width && 0 <= pt.Y && pt.Y <= canvas.Height)
            {
                const float speed = 0.001f;
                float curScale = transform.Elements[0];
                float nextScale = curScale + e.Delta * speed;
                nextScale = Math.Min(10, Math.Max(0.1f, nextScale));
                float scaleRatio = nextScale / curScale;
                Translate(-pt.X, -pt.Y);
                Zoom(scaleRatio, scaleRatio);
                Translate(pt.X, pt.Y);

                // ずれ防止
                prevMousePos = Point.Empty;
                canvas.Invalidate();
            }
        }

    }
}
