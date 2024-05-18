using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // начальные действия
            World.Init();
            pictureBoxScreen.Image = World.screen;
        }

        public void PictureBoxScreen_MouseDown(object s, MouseEventArgs e) // событие нажатия мышкой
        {         
            float positionX = (float)e.X / pictureBoxScreen.Width; // положение мышки по пикселям в 0-1
            float positionY = (float)e.Y / pictureBoxScreen.Height; // положение мышки по пикселям в 0-1
            bool isLeft = e.Button == MouseButtons.Left;

            World.Click(positionX, positionY, isLeft);
            
            pictureBoxScreen.Invalidate(); // Image поменялся, отобразить новое на экране
        }
    } // интерфейс

    public static class World
    {
        public static Checker[] checkers; // шашки в игре

        public static Bitmap screen; // текущий вид игры
        public static Graphics screenG; // для рисования на текущем виде игры

        public const float offsetX = 0.15f; // смещение до placeX 0
        public const float offsetY = 0.15f; // смещение до placeY 0
        public const float stepPlaceX = 0.1f; // расстояние между 2 placeX
        public const float stepPlaceY = 0.1f; // расстояние между 2 placeY

        public static void Init()
        {
            // создание шашек
            checkers = new Checker[16]
            {
                new Checker(0, 0, 1),
                new Checker(1, 0, 1),
                new Checker(2, 0, 1),
                new Checker(3, 0, 1),
                new Checker(4, 0, 1),
                new Checker(5, 0, 1),
                new Checker(6, 0, 1),
                new Checker(7, 0, 1),
                new Checker(0, 7, 2),
                new Checker(1, 7, 2),
                new Checker(2, 7, 2),
                new Checker(3, 7, 2),
                new Checker(4, 7, 2),
                new Checker(5, 7, 2),
                new Checker(6, 7, 2),
                new Checker(7, 7, 2),
            };

            // начальный вид игры
            screen = new Bitmap(1000, 1000);
            screenG = Graphics.FromImage(screen);
            ReDraw();
        } // создание шашек

        public static void Click(float positionX, float positionY, bool isLeft)
        {
            (int placeX, int placeY) = ConvertPositionToPlace(positionX, positionY);

            if (isLeft)
            {
                foreach (var c in checkers)
                    c.TrySelect(placeX, placeY);
            }
            else
            {
                foreach (var c in checkers)
                    c.TryMove(placeX, placeY);
            }

            ReDraw();
        } // при нажатии

        public static void ReDraw()
        {
            DrawOnPosition(Textures.checkersTable, 0.5f, 0.5f);
            foreach (Checker c in checkers)
                c.SelfDraw();
        } // перерисовка

        public static void DrawOnPosition(Bitmap b, float positionX, float positionY)
        {
            // 1000 - ширина и высота главного Bitmap
            screenG.DrawImage(b, positionX * 1000 - b.Width / 2f, positionY * 1000 - b.Height / 2f);
        } // нарисовать в центре указанного position

        public static void DrawOnPlace(Bitmap b, int placeX, int placeY)
        {
            (float positionX, float positionY) = ConvertPlaceToPosition(placeX, placeY);
            DrawOnPosition(b, positionX, positionY);
        } // нарисовать в центре указанного place

        public static (float, float) ConvertPlaceToPosition(int placeX, int placeY)
        {
            float positionX = offsetX + placeX * stepPlaceX; // смещение + количество * шаг
            float positionY = offsetY + placeY * stepPlaceY; // смещение + количество * шаг
            return (positionX, positionY);
        } // преобразовать place в position

        public static (int, int) ConvertPositionToPlace(float positionX, float positionY)
        {
            int placeX = (int)Math.Round((positionX - offsetX) / stepPlaceX);
            int placeY = (int)Math.Round((positionY - offsetY) / stepPlaceY);
            if (placeX < 0) placeX = 0;
            if (placeX > 7) placeX = 7;
            if (placeY < 0) placeY = 0;
            if (placeY > 7) placeY = 7;
            return (placeX, placeY);
        } // преобразовать position в place

        public static bool IsPlaceFree(int placeX, int placeY)
        {
            Checker checker = checkers.FirstOrDefault(c => c.placeX == placeX && c.placeY == placeY);
            return checker == null;
        } // свободно ли место

        public static bool IsPlaceBusy(int placeX, int placeY, out Checker che)
        {
            che = checkers.FirstOrDefault(c => c.placeX == placeX && c.placeY == placeY);
            return che != null;
        } // занято ли место и кем
    } // центральная логика

    public class Checker
    {
        public int placeX; // индекс ячейки по ширине
        public int placeY; // индекс ячейки по высота
        public int team; // команда (1/2)
        public bool isSelect; // выбрана ли сейчас эта шашка

        public Checker(int placeX, int placeY, int team)
        {
            // начальные значения при создании
            this.placeX = placeX;
            this.placeY = placeY;
            this.team = team;
            isSelect = false;
        }

        public void SelfDraw()
        {
            if (team == 1 && isSelect == false) World.DrawOnPlace(Textures.checkerWhite, placeX, placeY);
            if (team == 1 && isSelect == true) World.DrawOnPlace(Textures.checkerWhiteSelect, placeX, placeY);
            if (team == 2 && isSelect == false) World.DrawOnPlace(Textures.checkerBlack, placeX, placeY);
            if (team == 2 && isSelect == true) World.DrawOnPlace(Textures.checkerBlackSelect, placeX, placeY);
            if (isSelect) // отрисовка возможных ходов
            {
                if (team == 1 && placeX > 0 && placeY < 7 && World.IsPlaceFree(placeX - 1, placeY + 1))
                    World.DrawOnPlace(Textures.goal, placeX - 1, placeY + 1);
                if (team == 1 && placeX < 7 && placeY < 7 && World.IsPlaceFree(placeX + 1, placeY + 1))
                    World.DrawOnPlace(Textures.goal, placeX + 1, placeY + 1);
                if (team == 2 && placeX > 0 && placeY > 0 && World.IsPlaceFree(placeX - 1, placeY - 1))
                    World.DrawOnPlace(Textures.goal, placeX - 1, placeY - 1);
                if (team == 2 && placeX < 7 && placeY > 0 && World.IsPlaceFree(placeX + 1, placeY - 1))
                    World.DrawOnPlace(Textures.goal, placeX + 1, placeY - 1);

                if (team == 1 && placeX > 1 && placeY < 6 && World.IsPlaceFree(placeX - 2, placeY + 2) && World.IsPlaceBusy(placeX - 1, placeY + 1, out Checker c1) && c1.team == 2)
                    World.DrawOnPlace(Textures.goal, placeX - 2, placeY - 2);
                if (team == 1 && placeX < 6 && placeY < 6 && World.IsPlaceFree(placeX + 2, placeY + 2) && World.IsPlaceBusy(placeX + 1, placeY + 1, out Checker c2) && c2.team == 2)
                    World.DrawOnPlace(Textures.goal, placeX + 2, placeY + 2);
                if (team == 2 && placeX > 1 && placeY > 1 && World.IsPlaceFree(placeX - 2, placeY - 2) && World.IsPlaceBusy(placeX - 1, placeY - 1, out Checker c3) && c3.team == 1)
                    World.DrawOnPlace(Textures.goal, placeX - 2, placeY - 2);
                if (team == 2 && placeX < 6 && placeY > 1 && World.IsPlaceFree(placeX + 2, placeY - 2) && World.IsPlaceBusy(placeX + 1, placeY - 1, out Checker c4) && c4.team == 1)
                    World.DrawOnPlace(Textures.goal, placeX + 2, placeY - 2);
            }
        } // нарисовать себя на экране

        public void TrySelect(int clickPlaceX, int clickPlaceY)
        {
            isSelect = false; // сначала деактивироваться

            if (placeX == clickPlaceX && placeY == clickPlaceY) // если нажали сюда то активироваться
                isSelect = true;
        } // попробовать активироваться

        public void TryMove(int clickPlaceX, int clickPlaceY)
        {
            if (isSelect && World.IsPlaceFree(clickPlaceX, clickPlaceY))
            {
                if ((team == 1 && placeX - 1 == clickPlaceX && placeY + 1 == clickPlaceY) ||
                    (team == 1 && placeX + 1 == clickPlaceX && placeY + 1 == clickPlaceY) ||
                    (team == 2 && placeX - 1 == clickPlaceX && placeY - 1 == clickPlaceY) ||
                    (team == 2 && placeX + 1 == clickPlaceX && placeY - 1 == clickPlaceY))
                { placeX = clickPlaceX; placeY = clickPlaceY; isSelect = false; } // ход для передвижения

                Checker destroing = null;
                if ((team == 1 && placeX - 2 == clickPlaceX && placeY + 2 == clickPlaceY && World.IsPlaceBusy(placeX - 1, placeY + 1, out destroing) && destroing.team == 2) ||
                    (team == 1 && placeX + 2 == clickPlaceX && placeY + 2 == clickPlaceY && World.IsPlaceBusy(placeX + 1, placeY + 1, out destroing) && destroing.team == 2) ||
                    (team == 2 && placeX - 2 == clickPlaceX && placeY - 2 == clickPlaceY && World.IsPlaceBusy(placeX - 1, placeY - 1, out destroing) && destroing.team == 1) ||
                    (team == 2 && placeX + 2 == clickPlaceX && placeY - 2 == clickPlaceY && World.IsPlaceBusy(placeX + 1, placeY - 1, out destroing) && destroing.team == 1))
                { placeX = clickPlaceX; placeY = clickPlaceY; destroing.Destroy(); }
            }
        }

        public void Destroy()
        {
            World.checkers = World.checkers.Where(x => x != this).ToArray();
        }
    } // логика отдельной шашки

    public static class Textures
    {
        public static Bitmap checkerWhite = new Bitmap(Image.FromFile("checkerWhite.png"), 150, 150);
        public static Bitmap checkerBlack = new Bitmap(Image.FromFile("checkerBlack.png"), 150, 150);
        public static Bitmap checkerWhiteSelect = new Bitmap(Image.FromFile("checkerWhiteSelect.png"), 150, 150);
        public static Bitmap checkerBlackSelect = new Bitmap(Image.FromFile("checkerBlackSelect.png"), 150, 150);
        public static Bitmap checkersTable = new Bitmap(Image.FromFile("checkersTable.png"), 1000, 1000);
        public static Bitmap goal = new Bitmap(Image.FromFile("goal.png"), 80, 80);
    } // выдача текстур
}