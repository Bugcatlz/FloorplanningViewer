using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FloorplanningViewer_v2
{
    public partial class Form1 : Form
    {
        class Chip
        {
            int width;
            int height;
            Point point;
            Rectangle rectangle;
            List<Module> modules;
            public Chip(int width, int height, Point point)
            {
                this.width = width;
                this.height = height;
                modules = new List<Module>();
                this.point = point;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is Chip))
                {
                    return false;
                }

                Chip other = (Chip)obj;

                if (this.getPoint() == other.getPoint())
                    return true;
                else
                    return false;
            }

            public void setRectangle(int x, int y, int width, int height)
            {
                rectangle = new Rectangle(x, y, width, height);
            }

            public int getWidth()
            {
                return width;
            }

            public int getHeight()
            {
                return height;
            }

            public void addModule(Module module)
            {
                if (!modules.Contains(module))
                    modules.Add(module);
            }

            public void reMoveModule(Module module)
            {
                modules.Remove(module);
            }

            //回傳屬於這個chip且enable的modules
            public List<Module> getEnableModules()
            {
                List<Module> m = new List<Module>();
                for (int i = 0; i < modules.Count; i++)
                {
                    if (modules[i].getEnable())
                        m.Add(modules[i]);
                }
                return m;
            }

            public List<Module> getModules()
            {
                return modules;
            }

            public Rectangle getRectangle()
            {
                return rectangle;
            }

            public Point getPoint()
            {
                return point;
            }
        }
        class Module
        {
            private string name;
            protected Boolean enable = true;
            protected Color color;
            protected PointF center;
            protected List<KeyValuePair<Module, int>> connectedModules;
            protected GraphicsPath gp;
            protected RectangleF rectangle;
            protected bool enableConnection = false;
            public Module(string name)
            {
                this.name = name;
                connectedModules = new List<KeyValuePair<Module, int>>();
            }
            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is Module))
                {
                    return false;
                }
                Module module = (Module)obj;
                if (module.getName() == this.getName())
                    return true;
                else
                    return false;
            }

            public void addConnectedModule(Module module, int nets)
            {
                connectedModules.Add(new KeyValuePair<Module, int>(module, nets));
            }
            public string getName()
            {
                return name;
            }

            public List<KeyValuePair<Module, int>> getConnectedModules()
            {
                return connectedModules;
            }

            public PointF getCenter()
            {
                return center;
            }

            public GraphicsPath GetGraphicsPath()
            {
                return gp;
            }

            public void SetEnable(bool enable)
            {
                this.enable = enable;
            }

            public Boolean getEnable()
            {
                return enable;
            }

            public void setColor(Color c)
            {
                color = c;
            }

            public Color getColor()
            {
                return color;
            }

            public RectangleF getMinRectangle()
            {
                return new RectangleF();
            }

            public void setRectangleF(float x, float y, float w, float h)
            {
                rectangle = new RectangleF(x, y, w, h);
            }

            public RectangleF getRectangleF()
            {
                return rectangle;
            }

            public void setEnableConnection(bool enable)
            {
                enableConnection = enable;
            }

            public bool getEnableConnection()
            {
                return enableConnection;
            }
        }
        class Softmodule : Module
        {
            int minArea;
            double area;
            List<Point> corners;
            Rectangle boundingRect;
            double aspectRatio;
            double retangleRatio;

            public Softmodule(string name, int minArea)
            : base(name)
            {
                this.minArea = minArea;
            }



            public void setCorner(List<Point> points)
            {
                corners = new List<Point>(points);
            }

            public void setGraphicsPath(Graphics g, float bottomLeftX,float bottomLeftY, float chipUnitSize)
            {
                
                gp = new GraphicsPath();
                List<PointF> points = new List<PointF>();
                for (int i = 0; i < corners.Count; i++)
                {
                    float X = bottomLeftX + corners[i].X * chipUnitSize;
                    float Y = bottomLeftY - corners[i].Y * chipUnitSize;
                    points.Add(new PointF(X, Y));
                }
                for (int i = 1; i < points.Count; i++)
                    gp.AddLine(points[i - 1], points[i]);
                gp.CloseFigure();

                
            }

            public RectangleF getMinRectangle(float bottomLeftX, float bottomLeftY, float chipUnitSize)
            {
                Point minPoint = new Point(int.MaxValue, int.MaxValue);
                Point maxPoint = new Point(int.MinValue, int.MinValue);
                //找到左下角和右下角

                for (int i = 0; i < corners.Count; i++)
                {
                    Point point = corners[i];
                    if (minPoint.X > point.X)
                        minPoint.X = point.X;
                    if (minPoint.Y > point.Y)
                        minPoint.Y = point.Y;
                    if (maxPoint.X < point.X + 1)
                        maxPoint.X = point.X + 1;
                    if (maxPoint.Y < point.Y + 1)
                        maxPoint.Y = point.Y + 1;
                }
                int width = maxPoint.X - minPoint.X;
                int height = maxPoint.Y - minPoint.Y;
                return new RectangleF(bottomLeftX + minPoint.X*chipUnitSize
                    , bottomLeftY - (maxPoint.Y - 1) * chipUnitSize
                    , (width - 1) * chipUnitSize, (height - 1 ) * chipUnitSize);
            }

            public List<Point> getCorners()
            {
                return corners;
            }

            public void calculateArea()
            {
                //將最後一個座標加入以便計算面積
                corners.Add(corners[0]);

                //初始化面積
                area = 0;

                //利用外積求面積
                for (int i = 0; i < corners.Count - 1; i++)
                {
                    Point p1 = corners[i];
                    Point p2 = corners[i + 1];
                    area += p1.X * p2.Y - p2.X * p1.Y;
                }
                area /= 2;
                area = Math.Abs(area);

                //移除最後一個座標
                corners.RemoveAt(corners.Count - 1);
            }

            public void findBoundingRect()
            {
                calculateArea();
                //當大小為零時
                if (area == 0)
                {
                    center = new PointF(0, 0);
                    aspectRatio = 0;
                    retangleRatio = 0;
                    return;
                }
                Point minPoint = new Point(int.MaxValue, int.MaxValue);
                Point maxPoint = new Point(int.MinValue, int.MinValue);
                //找到左下角和右下角

                //找到左下角和右下角
                for (int i = 0; i < corners.Count; i++)
                {
                    if (minPoint.X > corners[i].X)
                        minPoint.X = corners[i].X;
                    if (minPoint.Y > corners[i].Y)
                        minPoint.Y = corners[i].Y;
                    if (maxPoint.X < corners[i].X)
                        maxPoint.X = corners[i].X;
                    if (maxPoint.Y < corners[i].Y)
                        maxPoint.Y = corners[i].Y;
                }
                //建立包含的最小矩形
                boundingRect = new Rectangle(minPoint.X, minPoint.Y, (maxPoint.X - minPoint.X), (maxPoint.Y - minPoint.Y));
                //建立中心點
                center = new PointF((minPoint.X + maxPoint.X) / 2f, (minPoint.Y + maxPoint.Y) / 2f);
                //根據最小矩形找到長寬比
                aspectRatio = Math.Round((float)boundingRect.Height / (float)boundingRect.Width, 2);
                //根據最小矩形的面積跟soft module的面積來建立面積比
                retangleRatio = Math.Round((float)area / ((float)(maxPoint.X - minPoint.X) * (maxPoint.Y - minPoint.Y)),2);
            }
            public int getMinArea()
            {
                return minArea;
            }

            public double getArea()
            {
                return area;
            }

            public double getAspectRatio()
            {
                return aspectRatio;
            }

            public double getRectangleRatio()
            {
                return retangleRatio;
            }

            public bool checkMinArea()
            {
                if (area >= minArea)
                    return true;
                else
                    return false;
            }

            public bool checkAspectRatio()
            {
                if (aspectRatio <= 2 && aspectRatio >= 0.5)
                    return true;
                else
                    return false;
            }

            public bool checkRectangleRatio()
            {
                if (retangleRatio <= 1 && retangleRatio >= 0.8)
                    return true;
                else
                    return false;
            }
        }

        class Fixedmodule : Module
        {
            Point point;
            int width;
            int height;

            public Fixedmodule(string name, Point point, int width, int height)
            : base(name)
            {
                this.point = point;
                this.width = width;
                this.height = height;
                this.color = Color.Black;
                center = new PointF(point.X + (float)width / 2, point.Y + (float)height / 2);
            }

            public RectangleF getMinRectangle(float bottomLeftX, float bottomLeftY, float chipUnitSize)
            {
                float x = bottomLeftX + point.X * chipUnitSize;
                // 且rectangle的起始座標是左上角，因此要再扣掉其高度，因為座標是右下角
                float y = bottomLeftY - (point.Y + getHeight()) * chipUnitSize;
                RectangleF rectangle = new RectangleF(x, y, getWidth() * chipUnitSize, getHeight() * chipUnitSize);
                return rectangle;
            }

            public void setGraphicsPath(Graphics g,float bottomLeftX, float bottomLeftY, float chipUnitSize)
            {
                gp = new GraphicsPath();
                RectangleF rectangle = getMinRectangle(bottomLeftX, bottomLeftY, chipUnitSize);
                gp.AddRectangle(rectangle);
            }

            public int getWidth()
            {
                return width;
            }

            public int getHeight()
            {
                return height;
            }

            public Point getPoint()
            {
                return point;
            }
        }

        //全域變數
        private List<Module> modules;
        private List<Softmodule> softmodules;
        private List<Fixedmodule> fixedmodules;
        private int chipHeight, chipWidth;
        private float chipUnitSize;
        private float bottomLeftX, bottomLeftY;
        private double HPWL;
        private double total;
        private bool connection;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Height = (int)(this.Size.Height * 0.9);
            textBox1.Visible = false;
        }

        //--------------------------------------
        //讀檔
        private void 匯入檔案ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //讀取輸入檔
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                softmodules = new List<Softmodule>();
                fixedmodules = new List<Fixedmodule>();
                using (StreamReader reader = new StreamReader(openFileDialog1.FileName))
                {
                    if(!readInput(reader))
                        return;
                }
            }
            else
                return;

            //讀取輸入檔
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(openFileDialog1.FileName))
                {
                    if(!readOutput(reader))
                        return ;
                }
            }
            else
                return;

            //建立List of module
            modules = new List<Module>(softmodules);
            modules.AddRange(fixedmodules);
            toolStripDropDownButton2.Enabled = true;
            //建立image
            Image img = new Bitmap(Math.Min(pictureBox1.Width, pictureBox1.Height), Math.Min(pictureBox1.Width, pictureBox1.Height));
            pictureBox1.Image = img;
            //設定module
            setModule();
            for (int i = 0; i < softmodules.Count; i++)
            {
                softmodules[i].calculateArea();
                softmodules[i].findBoundingRect();
            }

            draw();
            displaySoftModuleInfo();
            displayConnectionInfo();
            displayErrorMessage();
        }

        //讀取輸入檔
        private bool readInput(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                switch (tokens[0])
                {
                    case "CHIP":
                        chipWidth = int.Parse(tokens[1]);
                        chipHeight = int.Parse(tokens[2]);
                        chipUnitSize = (float)Math.Min(Math.Min(pictureBox1.Width, pictureBox1.Height) / (chipHeight*1.1F), Math.Min(pictureBox1.Width, pictureBox1.Height) / (chipWidth*1.1F));
                        bottomLeftX = (float)Math.Min(0.05 * chipHeight * chipUnitSize, 0.05 * chipWidth* chipUnitSize);

                        bottomLeftY = bottomLeftX + chipUnitSize* chipHeight;
                        break;
                    case "SOFTMODULE":
                        int softModuleCount = int.Parse(tokens[1]);
                        readInputSoftModule(reader, softModuleCount);
                        break;
                    case "FIXEDMODULE":
                        int fixedModuleCount = int.Parse(tokens[1]);
                        readInputFixedModule(reader, fixedModuleCount);
                        break;
                    case "CONNECTION":
                        int numConnection = int.Parse(tokens[1]);
                        readInputConnected(reader, numConnection);
                        break;
                    default:
                        MessageBox.Show("輸入檔格式錯誤", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                }
            }
            return true;
        }

        //讀取輸出檔
        private bool readOutput(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                switch (tokens[0])
                {
                    case "HPWL":
                        HPWL = double.Parse(tokens[1]);
                        break;
                    case "SOFTMODULE":
                        int softModuleCount = int.Parse(tokens[1]);
                        if (softModuleCount != softmodules.Count || !readOutputSoftModule(reader, softModuleCount))
                        {
                            MessageBox.Show("輸入檔與輸出檔不相符!!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                        break;
                    default:
                        MessageBox.Show("輸出檔格式錯誤!!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                }
            }
            return true;
        }

        //根據讀入的input檔來建立soft module的List
        private void readInputSoftModule(StreamReader reader, int softModuleCount)
        {
            for (int i = 0; i < softModuleCount; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                Softmodule softmodule = new Softmodule(tokens[0], int.Parse(tokens[1]));
                softmodules.Add(softmodule);
            }
        }

        //根據讀入的input來建立fixed module的List
        private void readInputFixedModule(StreamReader reader, int fixedModuleCount)
        {
            for (int i = 0; i < fixedModuleCount; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                Point point = new Point(int.Parse(tokens[1]), int.Parse(tokens[2]));
                Fixedmodule fixedmodule = new Fixedmodule(tokens[0], point, int.Parse(tokens[3]), int.Parse(tokens[4]));
                fixedmodules.Add(fixedmodule);
            }
        }

        //根據讀入的input來建立module之間的連線數量
        private void readInputConnected(StreamReader reader, int numConnection)
        {
            for (int i = 0; i < numConnection; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                Module module = findModuleByName(tokens[0]);
                module.addConnectedModule(findModuleByName(tokens[1]), int.Parse(tokens[2]));
            }
        }

        //根據讀入的input，來建立soft module的List中的conrner的位置
        private bool readOutputSoftModule(StreamReader reader, int softModuleCount)
        {
            for (int i = 0; i < softModuleCount; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(' ');
                Softmodule softmodule = (Softmodule)findModuleByName(tokens[0]);
                if (softmodule == null)
                    return false;
                int numberOfCorners = int.Parse(tokens[1]);
                List<Point> corners = new List<Point>();
                for (int j = 0; j < numberOfCorners; j++)
                {
                    line = reader.ReadLine();
                    tokens = line.Split(' ');
                    Point point = new Point(int.Parse(tokens[0]), int.Parse(tokens[1]));
                    corners.Add(point);
                }

                softmodule.setCorner(corners);

            }
            return true;
        }

        //根據module的名稱回傳對應的module
        private Module findModuleByName(string name)
        {
            for (int i = 0; i < fixedmodules.Count; i++)
            {
                if (fixedmodules[i].getName() == name)
                    return fixedmodules[i];
            }
            for (int i = 0; i < softmodules.Count; i++)
            {
                if (softmodules[i].getName() == name)
                    return softmodules[i];
            }
            return null;
        }
        //讀檔
        private void 匯入SA輸出檔ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        //-----------------------------------

        //-----------------------------------------
        //設定Moudle
        //根據傳入的數值回傳對應數量元素的List of color 
        private List<Color> generateColor(int n)
        {
            List<Color> rainbowColors = new List<Color>() {
            Color.Orange, Color.Yellow, Color.Green, Color.DeepSkyBlue,
            Color.Indigo, Color.Violet, Color.Pink, Color.CornflowerBlue, Color.DarkOrchid };
            List<Color> colors = new List<Color>();
            Random rand = new Random(1234);
            for (int i = 0; i < n; i++)
            {
                if (i < rainbowColors.Count)
                {
                    colors.Add(rainbowColors[i]);
                }
                else
                {
                    Color color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                    while (colors.Contains(color))
                    {
                        color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                    }
                    colors.Add(color);
                }
            }
            return colors;
        }

        //設定下拉式選單、module顏色、module的graphicspath
        private void setModule()
        {
            //建立modules的下拉式選單並綁定Event
            connection = false;
            toolStripDropDownButton2.DropDownItems.Clear();
            ToolStripMenuItem item1 = new ToolStripMenuItem("取消全選");
            item1.Click += new EventHandler(enableAllModule);
            toolStripDropDownButton2.DropDownItems.Add(item1);

            for (int i = 0; i < modules.Count; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(modules[i].getName());
                item.CheckOnClick = true;
                item.Checked = true;
                item.CheckedChanged += new EventHandler(enableModule);
                toolStripDropDownButton2.DropDownItems.Add(item);
            }

            List<Color> colors = generateColor(modules.Count);
            //設定modules的顏色
            for (int i = 0; i < softmodules.Count; i++)
            {
                softmodules[i].setColor(colors[i]);
            }

            for (int i = 0; i < fixedmodules.Count; i++)
            {
                fixedmodules[i].setColor(Color.Black);
            }

            Graphics g = pictureBox1.CreateGraphics(); // 取得 PictureBox 的 Graphics 物件

            //設定module的GraphicsPath
            for (int i = 0; i < softmodules.Count; i++)
                softmodules[i].setGraphicsPath(g, bottomLeftX,bottomLeftY, chipUnitSize);
            for (int i = 0; i < fixedmodules.Count; i++)
                fixedmodules[i].setGraphicsPath(g, bottomLeftX, bottomLeftY, chipUnitSize);

            //建立modules的下拉式選單並綁定Event
            toolStripDropDownButton3.DropDownItems.Clear();
            item1 = new ToolStripMenuItem("全選");
            item1.Click += new EventHandler(enableAllConnection);
            toolStripDropDownButton3.DropDownItems.Add(item1);
            for (int i = 0; i < modules.Count; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(modules[i].getName());
                item.CheckOnClick = true;
                item.Checked = false;
                item.CheckedChanged += new EventHandler(enableConnection);
                toolStripDropDownButton3.DropDownItems.Add(item);
            }
        }

        private void enableConnection(object sender, EventArgs e)
        {
            // 取得觸發事件的 ToolStripMenuItem
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            string name = item.Text;
            Module module = findModuleByName(name);

            // 根據 Checked 狀態設定
            if (item.Checked)
            {
                module.setEnableConnection(true);
            }
            else
            {
                module.setEnableConnection(false);
            }
            draw();
        }


        private void enableAllConnection(object sender, EventArgs e)
        {
            // 取得觸發事件的 ToolStripMenuItem
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;

            if (clickedItem.Text == "全選")
            {
                // 迭代所有的下拉菜單項目，除了點選的那個
                foreach (ToolStripMenuItem item in toolStripDropDownButton3.DropDownItems)
                {
                    if (item != clickedItem)
                    {
                        item.Checked = true; // 設置其他項目為未選中
                    }
                }
                clickedItem.Text = "取消全選";
            }
            else
            {
                // 迭代所有的下拉菜單項目，除了點選的那個
                foreach (ToolStripMenuItem item in toolStripDropDownButton3.DropDownItems)
                {
                    if (item != clickedItem)
                    {
                        item.Checked = false; // 設置其他項目為未選中
                    }
                }
                clickedItem.Text = "全選";
            }
        }

        //啟用module所觸發事件
        private void enableModule(object sender, EventArgs e)
        {
            // 取得觸發事件的 ToolStripMenuItem
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            string name = item.Text;
            Module module = findModuleByName(name);

            // 根據 Checked 狀態設定
            if (item.Checked)
            {
                module.SetEnable(true);
            }
            else
            {
                module.SetEnable(false);
            }
            draw();
        }

        //啟用所有module所觸發事件
        private void enableAllModule(object sender, EventArgs e)
        {
            // 取得觸發事件的 ToolStripMenuItem
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;

            if (clickedItem.Text == "全選")
            {
                // 迭代所有的下拉菜單項目，除了點選的那個
                foreach (ToolStripMenuItem item in toolStripDropDownButton2.DropDownItems)
                {
                    if (item != clickedItem)
                    {
                        item.Checked = true; // 設置其他項目為未選中
                    }
                }
                clickedItem.Text = "取消全選";
            }
            else
            {
                // 迭代所有的下拉菜單項目，除了點選的那個
                foreach (ToolStripMenuItem item in toolStripDropDownButton2.DropDownItems)
                {
                    if (item != clickedItem)
                    {
                        item.Checked = false; // 設置其他項目為未選中
                    }
                }
                clickedItem.Text = "全選";
            }
        }

        //設定Moudle
        //------------------------------------------
        //------------------------------------------
        //繪製module

        private void draw()
        {
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            drawChip(g);
            drawConnections(g);
            drawModule(g);
            儲存圖片ToolStripMenuItem.Enabled = true;
            pictureBox1.Refresh();

        }
        //繪製連線情況
        private void drawConnections(Graphics g)
        {
            int minConnection = int.MaxValue;
            int maxConnection = int.MinValue;

            // 尋找最小和最大連線數量
            for (int i = 0; i < modules.Count; i++)
            {
                List<KeyValuePair<Module, int>> keys = modules[i].getConnectedModules();
                for (int j = 0; j < keys.Count; j++)
                {
                    int connectionCount = keys[j].Value;
                    if (connectionCount < minConnection)
                        minConnection = connectionCount;
                    if (connectionCount > maxConnection)
                        maxConnection = connectionCount;
                }
            }

            //降低線段鋸齒狀情況
            g.SmoothingMode = SmoothingMode.AntiAlias;
            for (int i = 0; i < modules.Count; i++)
            {
                if (!modules[i].getEnableConnection())
                    continue;
                List<KeyValuePair<Module, int>> keys = modules[i].getConnectedModules();
                for (int j = 0; j < keys.Count; j++)
                {
                    int connectionCount = keys[j].Value;

                    // 根據連線數量取得漸變顏色
                    Color lineColor = GetGradientColor(connectionCount, minConnection, maxConnection);

                    Pen pen = new Pen(lineColor, 3);
                    PointF[] points = new PointF[2];
                    points[0] = new PointF(bottomLeftX + modules[i].getCenter().X * chipUnitSize, bottomLeftY - modules[i].getCenter().Y * chipUnitSize);
                    points[1] = new PointF(bottomLeftX + keys[j].Key.getCenter().X * chipUnitSize, bottomLeftY - keys[j].Key.getCenter().Y * chipUnitSize);
                    g.DrawCurve(pen, points);
                }
            }
        }

        // 根據連線數量取得漸變顏色
        private Color GetGradientColor(int value, int minValue, int maxValue)
        {
            // 自訂起始顏色和結束顏色
            Color startColor = Color.Orange;
            Color endColor = Color.Blue;

            // 根據連線數量在起始顏色和結束顏色之間進行插值
            float ratio = (float)(value - minValue + 1) / (maxValue - minValue + 1);
            int red = (int)(startColor.R + (endColor.R - startColor.R) * ratio);
            int green = (int)(startColor.G + (endColor.G - startColor.G) * ratio);
            int blue = (int)(startColor.B + (endColor.B - startColor.B) * ratio);

            return Color.FromArgb(red, green, blue);
        }


        private void drawSAModule(Graphics g)
        {

            for(int i=0;i<softmodules.Count;i++)
            {
                if (softmodules[i].getEnable())
                {
                    RectangleF[] rectangles = new RectangleF[1];
                    rectangles[0] = softmodules[i].getRectangleF();
                    Pen pen = new Pen(new SolidBrush(Color.Red), 2);
                    g.DrawRectangles(pen, rectangles);
                }
            }

            for (int i = 0; i < fixedmodules.Count; i++)
            {
                if (fixedmodules[i].getEnable())
                {
                    RectangleF[] rectangles = new RectangleF[1];
                    rectangles[0] = fixedmodules[i].getRectangleF();
                    Pen pen = new Pen(new SolidBrush(Color.Blue), 2);
                    g.DrawRectangles(pen, rectangles);
                }
            }

            StringFormat stringFormat = new StringFormat();
            // 設定文字置中
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            for (int i = 0; i < softmodules.Count; i++)
            {
                if (softmodules[i].getEnable())
                {
                    RectangleF rectangle = softmodules[i].getRectangleF();
                    // 計算字體大小，讓fixed module名字能完整顯示在rectangle內
                    Font font = new Font("Arial", 1);
                    while (g.MeasureString(softmodules[i].getName(), font).Width < rectangle.Width && g.MeasureString(softmodules[i].getName(), font).Height < rectangle.Height && font.Size < 21)
                    {
                        font = new Font("Arial", font.Size + 1, FontStyle.Bold);
                    }
                    font = new Font("Arial", font.Size - 1);

                    g.DrawString(softmodules[i].getName(), font, new SolidBrush(Color.Black), rectangle, stringFormat);
                }
            }

            for (int i = 0; i < fixedmodules.Count; i++)
            {
                if (fixedmodules[i].getEnable())
                {
                    RectangleF rectangle = fixedmodules[i].getRectangleF();
                    // 計算字體大小，讓fixed module名字能完整顯示在rectangle內
                    Font font = new Font("Arial", 1);
                    while (g.MeasureString(fixedmodules[i].getName(), font).Width < rectangle.Width && g.MeasureString(fixedmodules[i].getName(), font).Height < rectangle.Height && font.Size < 21)
                    {
                        font = new Font("Arial", font.Size + 1);
                    }
                    font = new Font("Arial", font.Size - 1);

                    g.DrawString(fixedmodules[i].getName(), font, new SolidBrush(Color.Black), rectangle, stringFormat);
                }
            }

            pictureBox1.Refresh();
        }
        //畫出網格
        private void drawChip(Graphics g)
        {
            Pen pen = new Pen(Color.Black, 5);
            RectangleF[] rectangles = new RectangleF[1];
            rectangles[0] = new RectangleF(bottomLeftX, bottomLeftY - chipHeight*chipUnitSize, chipWidth * chipUnitSize, chipHeight * chipUnitSize);
            g.DrawRectangles(pen, rectangles);
        }
        
        //畫出所有module
        private void drawModule(Graphics g)
        {
            for (int i = 0; i < softmodules.Count; i++)
            {
                if (softmodules[i].getEnable())
                {
                    GraphicsPath gp = softmodules[i].GetGraphicsPath();
                    Pen pen;
                    pen = new Pen(new SolidBrush(Color.Red), 1);
                    g.DrawPath(pen, gp);
                }
            }

            for (int i = 0; i < fixedmodules.Count; i++)
            {
                if (fixedmodules[i].getEnable())
                {
                    GraphicsPath gp = fixedmodules[i].GetGraphicsPath();
                    Pen pen;
                    pen = new Pen(new SolidBrush(Color.Blue), 1);
                    g.DrawPath(pen, gp);
                }
            }

            StringFormat stringFormat = new StringFormat();
            // 設定文字置中
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            for (int i =0;i<softmodules.Count;i++)
            {
                if (softmodules[i].getEnable())
                {
                    RectangleF rectangle = softmodules[i].getMinRectangle(bottomLeftX, bottomLeftY, chipUnitSize);
                    // 計算字體大小，讓fixed module名字能完整顯示在rectangle內
                    Font font = new Font("Arial", 1);
                    while (g.MeasureString(softmodules[i].getName(), font).Width < rectangle.Width && g.MeasureString(softmodules[i].getName(), font).Height < rectangle.Height && font.Size < 21)
                    {
                        font = new Font("Arial", font.Size + 1);
                    }
                    font = new Font("Arial", font.Size - 1);

                    g.DrawString(softmodules[i].getName(), font, new SolidBrush(Color.Black), rectangle, stringFormat);
                }
            }

            for(int i = 0; i < fixedmodules.Count; i++)
            {
                if (fixedmodules[i].getEnable())
                {
                    RectangleF rectangle = fixedmodules[i].getMinRectangle(bottomLeftX, bottomLeftY, chipUnitSize);
                    // 計算字體大小，讓fixed module名字能完整顯示在rectangle內
                    Font font = new Font("Arial", 1);
                    while (g.MeasureString(fixedmodules[i].getName(), font).Width < rectangle.Width && g.MeasureString(fixedmodules[i].getName(), font).Height < rectangle.Height && font.Size < 21)
                    {
                        font = new Font("Arial", font.Size + 1);
                    }
                    font = new Font("Arial", font.Size - 1);

                    g.DrawString(fixedmodules[i].getName(), font, new SolidBrush(Color.Black), rectangle, stringFormat);
                }
            }
        }

               
        //繪製module
        //------------------------------------------

        //-------------------------------------------
        //顯示module資訊

        //顯示soft module的資訊
        private void displaySoftModuleInfo()
        {
            dataGridView1.Rows.Clear();
            for (int i = 0; i < softmodules.Count; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                row.Cells[0].Value = softmodules[i].getName();
                row.Cells[1].Value = softmodules[i].getArea();
                row.Cells[2].Value = "(" + softmodules[i].getCenter().X.ToString() + ", "
                    + softmodules[i].getCenter().Y.ToString() + ")";
                row.Cells[3].Value = softmodules[i].getAspectRatio();
                row.Cells[4].Value = softmodules[i].getRectangleRatio();
                dataGridView1.Rows.Add(row);

                if (!softmodules[i].checkMinArea())
                {
                    dataGridView1.Rows[i].Cells[1].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold); ;
                    dataGridView1.Rows[i].Cells[1].Style.ForeColor = Color.Red;
                }
                if (!softmodules[i].checkAspectRatio())
                {
                    dataGridView1.Rows[i].Cells[3].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold); ;
                    dataGridView1.Rows[i].Cells[3].Style.ForeColor = Color.Red;
                }
                if (!softmodules[i].checkRectangleRatio())
                {
                    dataGridView1.Rows[i].Cells[4].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold); ;
                    dataGridView1.Rows[i].Cells[4].Style.ForeColor = Color.Red;
                }
            }
        }

        //顯示連線情形
        private void displayConnectionInfo()
        {
            total = 0;
            int numRow = 0;
            dataGridView2.Rows.Clear();
            for (int i = 0; i < modules.Count; i++)
            {
                List<KeyValuePair<Module, int>> keys = modules[i].getConnectedModules();
                for (int j = 0; j < keys.Count; j++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    double distance = Math.Abs(modules[i].getCenter().X - keys[j].Key.getCenter().X) + Math.Abs(modules[i].getCenter().Y - keys[j].Key.getCenter().Y);
                    row.CreateCells(dataGridView2);
                    numRow++;
                    for (int k = 0; k < 4; k++)
                    {
                        switch (k)
                        {
                            case 0:
                                row.Cells[0].Value = modules[i].getName() + "\r\n" + keys[j].Key.getName();
                                break;
                            case 1:
                                row.Cells[1].Value = keys[j].Value;
                                break;
                            case 2:
                                row.Cells[2].Value = distance;
                                break;
                            case 3:
                                double value = distance * keys[j].Value;
                                total += value;
                                row.Cells[3].Value = value;
                                break;
                        }
                    }
                    dataGridView2.Rows.Add(row);
                }
            }

            total = Math.Round(total, 1);

            DataGridViewRow lastRow = new DataGridViewRow();
            lastRow.CreateCells(dataGridView2);
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                    lastRow.Cells[i].Value = "總和";
                else if (i == 3)
                    lastRow.Cells[i].Value = total;
                else
                    lastRow.Cells[i].Value = "";
            }
            dataGridView2.Rows.Add(lastRow);

            if (total != HPWL)
            {
                dataGridView2.Rows[numRow].Cells[3].Style.Font = new Font(dataGridView1.Font, FontStyle.Bold); ;
                dataGridView2.Rows[numRow].Cells[3].Style.ForeColor = Color.Red;
            }
        }

        //顯示錯誤資訊
        private void displayErrorMessage()
        {
            textBox1.Visible = false;
            儲存錯誤ToolStripMenuItem.Enabled = false;
            string errorMessage = "錯誤清單:";
            int errorNum = 0;
            for (int i = 0; i < softmodules.Count; i++)
            {
                if (!softmodules[i].checkMinArea())
                    errorMessage += "\r\n" + (++errorNum).ToString() + ". " + softmodules[i].getName() +
                        "的面積為" + softmodules[i].getArea().ToString() + "，小於最小面積"
                        + softmodules[i].getMinArea();

                if (!softmodules[i].checkAspectRatio())
                    errorMessage += "\r\n" + (++errorNum).ToString() + ". " + softmodules[i].getName() +
                        "的長寬比例為" + softmodules[i].getAspectRatio().ToString() + "，不介於0.5-2之間";

                if (!softmodules[i].checkRectangleRatio())
                    errorMessage += "\r\n" + (++errorNum).ToString() + ". " + softmodules[i].getName() +
                        "的矩形比例為" + (softmodules[i].getRectangleRatio() * 100).ToString() + "%，不介於80%-100%之間";

                List<Point> corners = softmodules[i].getCorners();
                for (int j = 0; j < corners.Count; j++) 
                {
                    if (corners[j].X<0 || corners[j].X > chipWidth)
                    {
                        errorMessage += "\r\n" + (++errorNum).ToString() + ". " + softmodules[i].getName() +
                        "超出chip";
                    }
                    if (corners[j].Y < 0 || corners[j].Y > chipHeight)
                    {
                        errorMessage += "\r\n" + (++errorNum).ToString() + ". " + softmodules[i].getName() +
                        "超出chip";
                    }
                }
            }

            Graphics g = Graphics.FromImage(pictureBox1.Image);

            for (int i = 0;i<modules.Count;i++)
            {
                for(int j = i+1;j<modules.Count;j++)
                {
                    Region r1 = new Region(modules[i].GetGraphicsPath());
                    Region r2 = new Region(modules[j].GetGraphicsPath());
                    r1.Intersect(r2);
                    if(!r1.IsEmpty(g))
                    {
                        errorMessage += "\r\n"  + (++errorNum).ToString() + ". " + modules[i].getName() + "與重疊 " + modules[j].getName();
                    }
                }
            }

            if (total != HPWL)
                errorMessage += "\r\n" + (++errorNum).ToString() + ". 半周長導線為 " + HPWL.ToString("F1") +
                    "正確應該為 " + total.ToString("F1");


            if (errorNum != 0)
            {
                儲存錯誤ToolStripMenuItem.Enabled = true;
                textBox1.Visible = true;
                textBox1.Text = errorMessage;
                pictureBox1.Height = (int)(this.Size.Height * 0.9) - textBox1.Height;
            }
            else
            {
                textBox1.Visible = false;
                pictureBox1.Height = (int)(this.Size.Height * 0.9);
            }
        }


        //顯示module資訊
        //------------------------------------------- 

        //------------------------------------------
        //儲存檔案
        private void 儲存圖片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "PNG file|*.png";
            saveFileDialog1.FileName = "image.png";
            if (pictureBox1.Image != null)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    String output = saveFileDialog1.FileName;
                    pictureBox1.Image.Save(output);
                }
            }
        }

        private void 儲存錯誤ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog1.FileName = "errorMessage.txt";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write(textBox1.Text);
                }
            }
        }
        //儲存檔案
        //--------------------------------------------
    }
}
