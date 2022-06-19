using System;
using System.Drawing;
using System.Windows.Forms;

namespace VideoPlayerWatcher
{
    public partial class Form1 : Form
    {
        //Скриншоты
        Bitmap ScreenShot;
        Bitmap PrevScreenShot;

        //Координаты начала выделения
        int StartSelectionX;
        int StartSelectionY;

        //Заблокировать созранение параметров, пока они не загружены
        bool BlockSave = true;

        //Масштаб отображения скриншота
        //Так же используется для выделения рамки
        int scale = 1;

        //Признак того, что кнопка мыши зажата на пикчебоксе и идет выделение
        bool MouseDowned = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
        public void MakeScreenshot()
        {
            //Освобождаю память от предыдущего скриншота
            PrevScreenShot?.Dispose();

            //Копирую скриншот в предыдущий
            PrevScreenShot = ScreenShot;

            //далее получаю новый скриншот

            // получаем размеры виртуального экрана, содержащего все мониторы
            Rectangle bounds = SystemInformation.VirtualScreen;

            // создаем пустое изображения размером с экран устройства
            ScreenShot = new Bitmap(bounds.Width, bounds.Height);
            // создаем объект на котором можно рисовать
            using (var g = Graphics.FromImage(ScreenShot))
            {
                // перерисовываем экран на наш графический объект
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }

            //Устанавливаю ограничение на Вертикальные и горизонтальные числа
            VerticalTo.Maximum = ScreenShot.Height;
            HorizontalTo.Maximum = ScreenShot.Width;
            VerticalFrom.Maximum = ScreenShot.Height;
            HorizontalFrom.Maximum = ScreenShot.Width;
        }

        public bool CompareScreenShots()
        {
            //Если скриншота нет, то он не меняется
            if(PrevScreenShot == null)
                return false;

            //Сравниваю все пиксели 
            for (int i = (int)HorizontalFrom.Value; i < (int)HorizontalTo.Value; i++)
                for (int j = (int)VerticalFrom.Value; j < (int)VerticalTo.Value; j++)
                {
                    //сравниваю два пикселя с разных скриншотов
                    if(ScreenShot.GetPixel(i,j)!=PrevScreenShot.GetPixel(i,j))
                    {

                        return true;
                    }
                }
            return false;
        }

        //показать скриншот виртуального экрана
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ScreenShot != null)
            {
                //Вычислить оптимальные размеры
                float horizontal = ScreenShot.Width / pictureBox1.Width;
                float vertical = ScreenShot.Height / pictureBox1.Height;
                scale = (int)Math.Max(horizontal, vertical)+1;

                //Нарисовать уменьшенный скриншот и рамку выделения
                e.Graphics.DrawImage(ScreenShot, 0, 0, ScreenShot.Width / scale, ScreenShot.Height / scale);
                e.Graphics.DrawRectangle(Pens.Black,
                    (int)HorizontalFrom.Value/scale, (int)VerticalFrom.Value / scale, 
                    (int)(HorizontalTo.Value-HorizontalFrom.Value) / scale, (int)(VerticalTo.Value-VerticalFrom.Value) / scale);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //Изменено значение интервала
            //изменяем значение таймера
            timer1.Interval = (int)numericUpDown1.Value*1000;

            SaveValues();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Сделать скриншот
            MakeScreenshot();

            //Сравнить скриншоты и показать результат
            if (CompareScreenShots())
            {
                textBox1.AppendText(DateTime.Now.TimeOfDay.ToString() + " Видео идет\r\n");
                label3.Text = "Видео идет";
            }
            else
            {
                textBox1.AppendText(DateTime.Now.TimeOfDay.ToString() + " Видео остановлено\r\n");
                label3.Text = "Видео остановлено";
            }

            //Перерисовать скриншот
            pictureBox1.Invalidate();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                //Задаю начальные значения
                StartSelectionX = e.X;
                StartSelectionY = e.Y;
                VerticalFrom.Value = e.Y * scale;
                HorizontalFrom.Value = e.X * scale;
                VerticalTo.Value = e.Y * scale;
                HorizontalTo.Value = e.X * scale;
                //началось выделение
                MouseDowned = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void pictureBox1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            MouseDowned = false;
            SaveValues();
        }

        private void pictureBox1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Еси не идет выделение, но ничего не делаем
            if (!MouseDowned)
                return;
            //if (Mouse.LeftButton == MouseButtonState.Released)
            //    return;

            try
            {
                if(e.Y>StartSelectionY)
                {
                    VerticalFrom.Value = StartSelectionY * scale;
                    VerticalTo.Value = e.Y * scale;
                }
                else
                {
                    VerticalFrom.Value = e.Y * scale;
                    VerticalTo.Value = StartSelectionY * scale; 
                }
                if (e.X > StartSelectionX)
                {
                    HorizontalFrom.Value = StartSelectionX * scale;
                    HorizontalTo.Value = e.X * scale;
                }
                else
                {
                    HorizontalFrom.Value = e.X * scale;
                    HorizontalTo.Value = StartSelectionX * scale;
                }
            }
            catch (Exception ex)
            {

            }
            pictureBox1.Invalidate();
        }
        void SaveValues()
        {
            if (!BlockSave)
            {
                Properties.Settings.Default.Timeout = (int)numericUpDown1.Value;
                Properties.Settings.Default.HorizontalFrom = (int)HorizontalFrom.Value;
                Properties.Settings.Default.HorizontalTo = (int)HorizontalTo.Value;
                Properties.Settings.Default.VerticalFrom = (int)VerticalFrom.Value;
                Properties.Settings.Default.VerticalTo = (int)VerticalTo.Value;
                Properties.Settings.Default.Save();
            }
        }
        void LoadValues()
        {
            try
            {
                numericUpDown1.Value = Properties.Settings.Default.Timeout;
                HorizontalFrom.Maximum = Properties.Settings.Default.HorizontalFrom;
                HorizontalFrom.Value = Properties.Settings.Default.HorizontalFrom;

                HorizontalTo.Maximum = Properties.Settings.Default.HorizontalTo;
                HorizontalTo.Value = Properties.Settings.Default.HorizontalTo;

                VerticalFrom.Maximum = Properties.Settings.Default.VerticalFrom;
                VerticalFrom.Value = Properties.Settings.Default.VerticalFrom;

                VerticalTo.Maximum = Properties.Settings.Default.VerticalTo;
                VerticalTo.Value = Properties.Settings.Default.VerticalTo;
            }
            catch(Exception)
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadValues();
            //Пусть таймер сразу тикнет
            timer1_Tick(sender, e);

            //Пусть теперь можно сохранять
            BlockSave = false;
        }

        private void HorizontalTo_ValueChanged(object sender, EventArgs e)
        {
            SaveValues();
        }
    }
}
