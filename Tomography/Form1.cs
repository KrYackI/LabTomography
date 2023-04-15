using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tomography
{
    public partial class Form1 : Form
    {
        Bin Bin = new Bin();
        view view = new view();
        bool loaded = false;
        int currentLayer = 0;
        int FrameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        int min;
        int space;
        public Form1()
        {
            InitializeComponent();
        }

        void DisplayFPS()
        {
            if(DateTime.Now >= NextFPSUpdate)
            {
                this.Text = string.Format("CT Visualiser (fps={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                label1.Text = FrameCount.ToString();
                FrameCount = 0;
            }
            FrameCount++;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {   
                string str = dialog.FileName;
                Bin.readBin(str);
                view.setupview(glControl1.Width, glControl1.Height);
                loaded = true;
                trackBar1.Maximum = Bin.z - 1;
                trackBar3.Minimum = Bin.min();
                trackBar3.Maximum = Bin.max() - 255;
                trackBar4.Maximum = Bin.max() - 1 - Bin.min();
                trackBar4.Value = 2000;
                min = 0;
                space = 2000;
                glControl1.Invalidate();
                Form1_Load(sender, e);
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                draw(sender, e);
                glControl1.SwapBuffers();
            }
        }

        void draw(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                if (radioButton1.Checked) view.DrawStrips(currentLayer, min, space);
                if (radioButton2.Checked)
                {
                    if (needReload)
                    {
                        view.generateTextureImage(currentLayer, min, space);
                        view.Load2DTexture();
                        needReload = false;
                    }
                    view.DrawTexture();
                }
                if (radioButton3.Checked) view.DrawQuads(currentLayer, min, space);
            }
/*            Form1_Load(sender, e);*/
            needReload = true;
        }

        void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            glControl1.Invalidate();
/*            draw(sender, e);*/
            /*            if (loaded)
                        {   
                            if (radioButton3.Checked)view.DrawQuads(currentLayer);
                            if (radioButton2.Checked) view.generateTextureImage(currentLayer);
                            glControl1.SwapBuffers();
                        }
                        Form1_Load(sender, e);
                        needReload = true;*/
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                DisplayFPS();
                glControl1.Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }
        bool needReload = false;

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                radioButton1.Checked = false;
                radioButton2.Checked = false;
            }
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            min = trackBar3.Value;
            glControl1.Invalidate();
            /*            draw(sender, e);*/
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            space = trackBar4.Value;
            glControl1.Invalidate();
            /*            draw(sender, e);*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            short min = Bin.min();
            button1.Text = min.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            short max = Bin.max();
            button2.Text = max.ToString();
        }
    }

    class Bin
    {
        public static int x, y, z;
        public static short[] array;
        public Bin() { }
        public void readBin(string Path)
        {
            if (File.Exists(Path))
            {
                BinaryReader reader = new BinaryReader(File.Open(Path, FileMode.Open));

                x = reader.ReadInt32();
                y = reader.ReadInt32();
                z = reader.ReadInt32();

                int arraysize = x * y * z;
                array = new short[arraysize];
                for (int i = 0; i < arraysize; i++)
                    array[i] = reader.ReadInt16();
            }
        }

        public static short min()
        {
            short min = short.MaxValue;
            int arraysize = x * y * z;
            for (int i = 0; i < arraysize; i++)
                if (array[i] < min)
                    min = array[i]; 
                return min;
        }

        public static short max()
        {
            short max = short.MinValue;
            int arraysize = x * y * z;
            for (int i = 0; i < arraysize; i++)
                if (array[i] > max)
                    max = array[i];
            return max;
        }

    }

    class view
    {
        
        public view() { }
        public int clamp(int val, int min, int max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }
        public void setupview(int width, int heigth)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.x, 0, Bin.y, -1, 1);
            GL.Viewport(0, 0, width, heigth);
        }
        public Color transferfunction(short value, int _min, int space)
        {
            int min = _min;
            int max = min + space;
            int newVal = clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }
        public void DrawQuads(int layerNumber, int min, int space)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(PrimitiveType.Quads);
            for (int x_coord = 0; x_coord < Bin.x - 1; x_coord++)
                for (int y_coord = 0; y_coord < Bin.y - 1; y_coord++)
                {
                    short value;
                    value = Bin.array[x_coord + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + (y_coord + 1) * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord, y_coord + 1);

                    value = Bin.array[x_coord + 1 + (y_coord + 1) * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord + 1, y_coord + 1);

                    value = Bin.array[x_coord + 1 + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
            GL.End();
        }

        public void DrawStrips(int layerNumber, int min, int space)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            short value;
            int x_coord = 0;
            int y_coord = 0;
            for (x_coord = 0; x_coord < Bin.x - 1; x_coord++)
            {
                GL.Begin(PrimitiveType.QuadStrip);
                for (y_coord = 0; y_coord < Bin.y - 1; y_coord++)
                {
                    value = Bin.array[x_coord + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord, y_coord);

                    value = Bin.array[x_coord + 1 + y_coord * Bin.x + layerNumber * Bin.x * Bin.y];
                    GL.Color3(transferfunction(value, min, space));
                    GL.Vertex2(x_coord + 1, y_coord);
                }
                GL.End();
            }
        }

        int VBOtexture;
        Bitmap textureImage;
        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0,0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb );
            GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,
                data.Width,data.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            ErrorCode Er = GL.GetError();
            string str = Er.ToString();
        }
        public void generateTextureImage(int layerNumber, int min, int space)
        {
            textureImage = new Bitmap(Bin.x, Bin.y);
            for (int i = 0; i < Bin.x; ++i) 
                for(int j=0;j<Bin.y;++j)
                {
                    int pixelNumber = i + j * Bin.x + layerNumber * Bin.x * Bin.y;
                    textureImage.SetPixel(i, j, transferfunction(Bin.array[pixelNumber], min, space));
                }
        }
    

        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.x, Bin.y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.x, 0);
            GL.End();

            GL.Disable(EnableCap.Texture2D);

        }
    }
    



}
