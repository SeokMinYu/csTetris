using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Tetris_ysm
{
    public partial class Form1 : Form
    {
        Game game;
        Board CNT;
        int b_x;//보드 폭
        int b_y;//보드 높이
        int b_width; //X좌표 1의 x Pixels
        int b_height;//Y좌표 1의 y Pixels
        private bool isPlay;

        // 기본 아이피를 설정할 변수 설정 
        private string _svrIP = "";
        // 기본 포트를 설정할 변수와 포트 설정  
        private int _svrPort = 2005;

        // 소켓 통신을 할 클라이언트 소켓 
        private TcpClient _tcpClient = null;
        // 별칭 
        private string _nick = "user";

        // 네트워크 스트림 객체를 선언합니다. 
        private NetworkStream _netStream = null;
        // 읽기 스트림 객체 선언 
        private StreamReader _stmReader = null;
        // 쓰기 스트림 객체 선언 
        private StreamWriter _stmWriter = null;

        // 통신을 중단했는지를 나타내는 플래그 
        private bool _isStop = false;

        // 생성한 폼 어플리케이션에 로그를 찍기위해 선언한 delegate 
        public delegate void LogWriteDelegate(string msg);

        public Form1()
        {
            InitializeComponent();

            // 현재 컴퓨터의 아이피를 기본적으로 보여줍니다.
            IPHostEntry localHostEntry = Dns.GetHostByName(Dns.GetHostName());
            // 현재 컴퓨터의 아이피를 기본 아이피로 설정 
            _svrIP = localHostEntry.AddressList[0].ToString();
            // 아이피 상자에 보여줍니다.
            txtIP.Text = _svrIP;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Reset();
            CNT = Board.GameBoard;
            game = Game.Singleton;
            b_width = GameRule.B_WIDTH;
            b_height = GameRule.B_HEIGHT;
            b_x = GameRule.B_X;
            b_y = GameRule.B_Y;
            SetClientSizeCore(20 * b_width, b_y * b_height);
            
        }

        private void Reset()
        {
            isPlay = false;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (isPlay)
            {
                DrawGraduation(e.Graphics);
                DrawDiagram(e.Graphics);
                //고속복사 깜빡이 방지
                DoubleBuffered = true;
                // 쌓이는 벽돌 색칠
                DrawBoard(e.Graphics);
            }
        }

        private void DrawBoard(Graphics graphics)
        {
            for(int xx = 0; xx < b_x; xx++)
            {
                for(int yy = 0; yy < b_y; yy++)
                {
                    if(game[xx, yy] != 0)
                    {
                        Rectangle now_rt = new Rectangle(xx * b_width + 2, yy * b_height + 2, b_width - 4, b_height - 4);
                        graphics.DrawRectangle(Pens.Black, now_rt);
                        graphics.FillRectangle(Brushes.Red, now_rt);
                    }
                }
            }
        }
        private void DrawDiagram(Graphics graphics) // 도형그리기
        {
            Pen dpen = new Pen(Color.Red, 4); //두께가 4
            Point now = game.NowPosition; //포지션 지정
            // 몇번째 블록인지 판단
            // 몇번 블록인지
            int b_n = game.BlockNum;
            // 현재 회전을 몇번했는지
            int t_n = game.Turn;
            // 4*4의 도형을 그려줌
            for(int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    if(BlockValue.b_vals[b_n,t_n,xx,yy] != 0)// 블록이 있는지 없는지 판단
                    {
                        Rectangle now_rt = new Rectangle((now.X+xx)* b_width + 2, (now.Y+yy) * b_height + 2, b_width - 4, b_height - 4);
                        graphics.DrawRectangle(dpen, now_rt);
                    }
                }
            }
            
        }

        private void DrawGraduation(Graphics graphics) //라인그리기
        {
            DrawHorizon(graphics); // 수직선
            DrawVerticals(graphics); // 수평선

        }
        private void DrawHorizon(Graphics graphics) // 수직선
        {
            Point st = new Point();
            Point et = new Point();
            Pen pLine = new Pen(Color.LightGray, 1);
            pLine.DashStyle = DashStyle.DashDot;

            for (int C__Y = 0; C__Y < b_y; C__Y++)
            {
                st.X = 0;
                st.Y = C__Y * b_height;
                et.X = b_x * b_width;
                et.Y = st.Y;

               

                graphics.DrawLine(pLine, st, et);
            }
        }
        private void DrawVerticals(Graphics graphics)  // 수평선
        { // 선을 그려줄 두개의 포인트
            Point st = new Point();
            Point et = new Point();
            Pen pLine = new Pen(Color.LightGray, 1);
            pLine.DashStyle = DashStyle.DashDot;

            for (int C__X = 0; C__X < b_x; C__X++)
            {
                st.X = C__X * b_width;
                st.Y = 0;
                et.X = st.X;
                et.Y = b_y * b_height;
                graphics.DrawLine(pLine, st, et);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPlay)
            {
                switch (e.KeyCode)
                {
                    case Keys.Right: MoveRight(); break;
                    case Keys.Left: MoveLeft(); break;
                    case Keys.Down: MoveDown(); break;
                    case Keys.Up: MoveTurn(); break;
                    case Keys.F1: Control_Input(); break;
                    // 한번에 다운
                    case Keys.Space: MoveSpaceDown(); break;
                }
                Invalidate();
            }
        }
        private void Control_Input() // Q입력
        {
            txtSend.Enabled = true;
            txtOutput.Enabled = true;
            button3.Enabled = true;
            label8.Visible = true;
            Region rg = MakeRegion();
            // Invalidate : 화면갱신
            Invalidate(rg);
        }

        private void MoveSpaceDown() //한번에 다운
        {
            while (game.MoveDown())
            {
                Region rg = MakeRegion(0, -1); //y감소
                Invalidate(rg);
            }
            EndingCheck();
        }

        private void MoveTurn() // 회전
        {
            if(game.MoveTurn())
            {
                // 인자가 없는
                Region rg = MakeRegion();
                // Invalidate : 화면갱신
                Invalidate(rg);
            }
        }
        private void MoveDown() //다운
        {
            if (game.MoveDown())
            {
                Region rg = MakeRegion(0, -1); //y감소
                Invalidate(rg);
            }
            else
            {
                EndingCheck();
            }
        }
        private void EndingCheck()
        {
            if (isPlay)
            {
                if (game.Next())
                {
                    Invalidate();
                    // 블록이 지워질때만 점수 증가
                    if (CNT.cnt != 0)
                    {
                        label2.Text = Convert.ToString(int.Parse(label2.Text) + (CNT.cnt * 20));
                        SendMessage(label2.Text);
                    }
                }
                else
                {
                    timer_down.Enabled = false;
                    
                    if (timer_down.Interval == 1)
                    {
                        DialogResult re = MessageBox.Show("테트리스 게임시작", "게임시작", MessageBoxButtons.YesNo);

                        if (re == DialogResult.Yes)
                        {
                            SendMessage("게임시작");
                            timer_down.Interval = 400;
                            game.Restart();
                            timer_down.Enabled = true;
                            Invalidate();
                        }
                        else
                        {
                            Close();
                        }
                    }
                    else
                    {
                        DialogResult re = MessageBox.Show("다시 시작", "게임오버", MessageBoxButtons.YesNo);

                        if (re == DialogResult.Yes)
                        {
                            game.Restart();
                            timer_down.Enabled = true;
                            if (Convert.ToInt32(label4.Text) < Convert.ToInt32(label2.Text))
                            {
                                label4.Text = label2.Text;
                            }
                            label2.Text = "0";
                            Invalidate();
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            }
            
        }
        private void MoveLeft() // 왼쪽이동
        {
            if (game.MoveLeft())
            {
                Region rg = MakeRegion(1, 0); //x증가
                Invalidate(rg);
            }
        }

        private void MoveRight() // 오른쪽이동
        {
            if (game.MoveRight())
            {
                Region rg = MakeRegion(-1, 0); //x감소
                Invalidate(rg);
            }
        }
        private Region MakeRegion(int cx, int cy) //다시그려줄 영역
        {
            Point now = game.NowPosition;
            // 현재 포지션과 블록번호, 회전정보
            int b_n = game.BlockNum;
            int t_n = game.Turn;

            Region region = new Region();
            if (isPlay)
            {
                for (int xx = 0; xx < 4; xx++)
                {
                    for (int yy = 0; yy < 4; yy++)
                    {
                        if (BlockValue.b_vals[b_n, t_n, xx, yy] != 0)// 블록의 위치 판단하여 도형형태 생성
                        {
                            // 현재 사각형 영역
                            Rectangle rect1 = new Rectangle((now.X + xx) * b_width, (now.Y + yy) * b_height, b_width, b_height);

                            // 이전 사걱형 영역
                            Rectangle rect2 = new Rectangle((now.X + cx + xx) * b_width, (now.Y + cy + yy) * b_height, b_width, b_height);
                            Region rg1 = new Region(rect1);
                            Region rg2 = new Region(rect2);
                            // 각각의 리전 결합
                            region.Union(rg1);
                            region.Union(rg2);
                        }
                    }
                }
            }
            return region;
        }
        private Region MakeRegion() // 회전
        {
            Point now = game.NowPosition;
            // 현재 포지션과 블록번호, 회전정보
            int b_n = game.BlockNum;
            int t_n = game.Turn;
            // 이전 턴에대한 부분
            int old_t_n = (t_n + 3) % 4;
            Region region = new Region();

            if (isPlay)
            {
                for (int xx = 0; xx < 4; xx++)
                {
                    for (int yy = 0; yy < 4; yy++)
                    {
                        if (BlockValue.b_vals[b_n, t_n, xx, yy] != 0)// 블록의 위치 판단하여 도형형태 생성
                        {
                            // 현재 사각형 영역
                            Rectangle rect1 = new Rectangle((now.X + xx) * b_width, (now.Y + yy) * b_height, b_width, b_height);
                            Region rg1 = new Region(rect1);
                            // 각각의 리전 결합
                            region.Union(rg1);
                        }
                        if (BlockValue.b_vals[b_n, old_t_n, xx, yy] != 0)// 턴하고 다음
                        {
                            // 현재 사각형 영역
                            Rectangle rect1 = new Rectangle((now.X + xx) * b_width, (now.Y + yy) * b_height, b_width, b_height);
                            Region rg1 = new Region(rect1);
                            // 각각의 리전 결합
                            region.Union(rg1);
                        }
                    }
                }
            }
            return region;
        }
        private void timer_down_Tick(object sender, EventArgs e)
        {
            if (isPlay)
            {
                MoveDown();
            }
        }
        private void OffController()
        {
            txtIP.Enabled = false;
            button1.Enabled = false;
            txtNick.Enabled = false;
            txtSend.Enabled = false;
            txtOutput.Enabled = false;
            button3.Enabled = false;
        }

        private void OnController()
        {
            txtIP.Enabled = true;
            button1.Enabled = true;
            txtNick.Enabled = true;
            txtSend.Enabled = true;
            txtOutput.Enabled = true;
        }

        // 시작하기 버튼
        private void button2_Click(object sender, EventArgs e)
        {
            timer_down.Interval = 1;
            OffController();
            isPlay = true;
            Invalidate();
            button2.Visible = false;
        }

        // 연결하기 
        private void btnConn_Click(object sender, EventArgs e)
        {
            // 아이피의 값이 비어있지 않다면  
            if (txtIP.Text != string.Empty)
                // 현재 설정된 아이피를 서버에 접속할 아이피로 설정 
                _svrIP = txtIP.Text;
            else
                // 비어있다면 입력하도록 메세지를 보여줌 
                MessageBox.Show("주소를 입력 하여 주십시요");

            // 별칭 입력 상자가 비어있지 않다면 
            if (txtNick.Text != string.Empty)
                // 입력한 값으로 별칭 사용 
                _nick = txtNick.Text;
            else
                // 설정하지 않았다면 아이피를 별치으로 사용 
                _nick = _svrIP;

            // 서버로 메세지를 받을 쓰레드를 실행합니다.
            Thread _clitThread = new Thread(new ThreadStart(ClientReceive));
            // 즉시 중지할 수 있도록 백그라운드로 실행 합니다. 
            _clitThread.IsBackground = true;
            // 쓰레드를 실행 합니다. 
            _clitThread.Start();
        }


        // 어플리 케이션의 쓰레드에 포함하기 위해 델리게이트 이용
        public void MessageWrite(string msg)
        {
            // 소켓 쓰레드와 어플리케이션 쓰레드와 충돌되지 않도록 데리게이트 이용 
            LogWriteDelegate deleLogWirte = new LogWriteDelegate(AppendMsg);
            // 어플리케이션의 Invoke를 사용하여 델리게이트를 실행 
            this.Invoke(deleLogWirte, new object[] { msg });
        }


        // 화면의 대화창에 메세지를 출력합니다. 
        public void AppendMsg(string msg)
        {
            // 메세지 추가와 함께 개행되도록 한다. 
            txtOutput.AppendText(msg + "\r\n");

            // 메세지창에 포커스를 줌 
            txtOutput.Focus();
            // 메세지 추가된 부분까지 스크롤시켜 보여줌 
            txtOutput.ScrollToCaret();
            // 다시 입력할수 있도록 메세지 입력 상자에 포커스
            txtSend.Focus();
        }


        // 쓰기 스트림으로 메세지를 전송합니다. 
        public void SendMessage(string msg)
        {
            // 쓰기 스트림이 유효한지를 체크합니다. 
            if (_stmWriter != null)
            {
                // 누가보냈는지와 메세지 내용을 조합하여 쓰기 스트림에 씁니다. 
                _stmWriter.WriteLine("[" + _nick + "] " + msg);

                // 특정단어로 블럭제어
                if (isPlay)
                {
                    if (msg == "아래")
                    {
                        MoveDown(); 
                    }
                    else if (msg == "왼")
                    {
                        MoveLeft();
                    }
                    else if (msg == "오른")
                    {
                        MoveRight();
                    }
                    else if (msg == "위")
                    {
                        MoveTurn();
                    }
                    else if (msg == "스페이스")
                    {
                        MoveSpaceDown();
                    }
                    Invalidate();
                }

                // 쓰기 스트림에 있는 내용을 방출합니다. 
                _stmWriter.Flush();
            }
        }


        // 쓰레드에서 실행된 쓰레드 입니다.
        private void ClientReceive()
        {
            try
            {
                // 클라이언트 소켓을 생성, 연결합니다. 
                _tcpClient = new TcpClient(_svrIP, _svrPort);

                // 클라이언트가 연결 되었다면 
                if (_tcpClient.Connected)
                {
                    // 네트워크 스트림을 생성합니다. 
                    _netStream = _tcpClient.GetStream();
                    // 읽기 스트림을 생성합니다.
                    _stmReader = new StreamReader(_netStream);
                    // 쓰기 스트림을 생성합니다.
                    _stmWriter = new StreamWriter(_netStream);
                    SendMessage("님이 접속하였습니다.");

                    while (!_isStop)
                    {
                        // 읽기 스트림으로 부터 메세지를 읽어드림
                        string rcvMsg = _stmReader.ReadLine();
                        // 로그창에 서버로부터 받은 메세지를 추가합니다. 
                        MessageWrite(rcvMsg);
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                // 접속이 끊겼음을 알립니다.
                MessageWrite("접속이 끊겼습니다.");
            }
            finally
            {
                // 읽기 스트림을 닫습니다. 
                _stmReader.Close();
                // 쓰기 스트림을 닫습니다. 
                _stmWriter.Close();
                // 네트워크 스트림을 닫습니다. 
                _netStream.Close();
                // 클라이언트 소켓을 닫습니다. 
                _tcpClient.Close();
            }
        }


        // 입력상자에서 엔터를 치면 메세지를 전송합니다. 
        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 엔터를 입력하였다면 
            if (e.KeyChar == '\r')
            {
                // 입력상자에 입력한 스트링을 받습니다.
                string msg = txtSend.Text;

                // 서버로 메세지를 전송합니다. 
                SendMessage(msg);
                // 입력상자의 내용을 지웁니다.
                txtSend.Clear();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OffController();
            isPlay = true;
            button3.Enabled = false;
            label8.Visible = false;
            Invalidate();
        }
    }
}
