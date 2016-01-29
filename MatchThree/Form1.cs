using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using MatchThree.Properties;
using System.Media;
using System.Timers;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        System.Timers.Timer sessionTimer;
        public const int BOARDWIDTH = 10, BOARDHEIGHT = 10; 
        Random random = new Random();  //Used to assign new Jewel Types 
        Point lastEvent = new Point(0,0); //Used in detecting user selecting two objects and in highlighting first choice.
        struct gamePiece //Stores the jewels and buttons
        {
            public Button pieceButton;
            public int jewelType; //Used to Identify which jewel it is
        }
        TableLayoutPanel gameBoard; //Holds all the gamePieces that make up the board. 
        Label highScoreLabel, multiplierLabel, sessionLabel; //Show the current Score and Multiplier
        Button tutorialButton; //Used to restart and look up the rules
        static System.Windows.Forms.Timer multiTimer; //Used to count down the multiplier
        gamePiece[,] pieceArray = new gamePiece[BOARDWIDTH, BOARDHEIGHT]; //The array of the jewels in game
        string rules;
        FlowLayoutPanel leftPanel; //Holds score and multiplier
        private AxWMPLib.AxWindowsMediaPlayer musicPlayer; //Plays the theme on loop
        SoundPlayer sfx; //Used for sound effects 
        long score; //Stores the current score.
        float multiplier; //Stores the current multiplier 
        int sessionTime;
        //Creates the main form and calls methods to add all the components in
        public Form1()
        {
            rules = "Match 3 to create a chain, the bigger the chain, the more score you get. \n\nYou can also try finding them in quick succession in order to maximize your multiplier!";
            InitializeComponent();
            createMusic(); //Makes sure music is loaded starts it's first play
            startScreen(); //Creates the start Screen
            createBoard();  //Initializes and configures gameBoard
            createPieces(); //Creates all the pieces, adds them to gameBoard and creates the events for handling clicking
            createLeftSide(); //Initializes the left panel and adds it to the main form
            createHighScore(); //Creates the high score and multiplier labels
            createLeftButtons(); //Creates the left side buttons and specifically the tutorial button
            updateLeft(); //Updates the text on all the left side labels
            
        }

        //Initializes the music player 
        //Passes it the URL for the music (badly done)
        private void createMusic()
        {
            musicPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            musicPlayer.CreateControl();
            musicPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(this.musicPlayer_PlayStateChange);
            musicPlayer.URL = "C:\\Users\\arturpopov\\Desktop\\C#\\projectSharp\\MatchThree\\WindowsFormsApplication1\\WindowsFormsApplication1\\Resources\\song18_0.mp3";
            musicPlayer.Ctlcontrols.play();
            
            sfx = new SoundPlayer();
            sfx.LoadCompleted += new AsyncCompletedEventHandler(this.loadedSFX);

            //First sound. Makes sure there is a sfx for the first match
            sfx.Stream = Resources.Item1A;
            
        }
        //THE BEGINNING SCREEN
        //Sets the timer and it's intervals. 
        private void startScreen()
        {
            MessageBox.Show("WELCOME TO 'QUEST FOR GENERIC JEWELS'");
            sessionTimer = new System.Timers.Timer();
            sessionTimer.Interval = 1000;

            sessionTimer.Elapsed += new ElapsedEventHandler(session_Checker);
       
            sessionTime = 30;
           
     
        }

        //Fires when timer Inverval fires
        //Invokes the updateLeft method in order to show the change in time
        //Initializes a new session if the timer ran out
        private void session_Checker(object sender, ElapsedEventArgs e)
        {
            if (sessionTime > 0)
            {
                sessionTime--;
                this.Invoke(new Action(() => this.updateLeft()));
            }
            else
            {
                sessionTimer.Stop();
                MessageBox.Show("You've run out of time with a score of " + Convert.ToString(score) + "\n Click OK to try Again!");
                sessionTime = 30;
                score = 0;
                multiplier = 1;
                this.Invoke(new Action(() => this.updateLeft()));
            }
        }
        //Loads a random SFX to the sfx. 
        private void sfx_Play()
        {
            switch (random.Next(10, 13))
            {
                case 10:
                    sfx.Stream = Resources.Item1A;
                    break;
                case 11:
                    sfx.Stream = Resources.Item1B;
                    break;
                case 12:
                    sfx.Stream = Resources.Menu1A;
                    break;
                case 13:
                    sfx.Stream = Resources.Menu1B;
                    break;
            }
            sfx.Load();
            
        }
        //Plays the preloaded sfx
        private void loadedSFX(object sender, AsyncCompletedEventArgs e)
        {
            sfx.Play();
        }

        //Makes sure the music continues on loop
        void musicPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
           if(e.newState == 8 || e.newState == 1) //If it's stopped or paused
           {
               musicPlayer.Ctlcontrols.play();
           }
        }

        private void createBoard()
        {
            //Set up the Board
            gameBoard = new TableLayoutPanel();
            gameBoard.Location = new System.Drawing.Point(250, 25);
            gameBoard.Name = "gameBoard";
            gameBoard.Size = new System.Drawing.Size(500, 500);

            gameBoard.TabIndex = 0;
            gameBoard.RowCount = BOARDHEIGHT;
            gameBoard.ColumnCount = BOARDWIDTH;

            //Remove Default Padding
            gameBoard.Margin = new Padding(0);
            gameBoard.AutoSize = false;

            //Add the board to the Form
            Controls.Add(gameBoard);
        }

        private void createLeftButtons()
        {
            //CREATE THE LEFT BUTTON
            tutorialButton = new Button();
            tutorialButton.Location = new Point(15, 510);
            tutorialButton.Size = new System.Drawing.Size(200, 30);
            tutorialButton.Text = "How do I PLAY?!";
            tutorialButton.FlatStyle = FlatStyle.Flat;
            tutorialButton.Click += new EventHandler(showHowToPlay);
            this.Controls.Add(tutorialButton);
            tutorialButton.BringToFront();
            sessionLabel.BringToFront();
        }
        //Explains how to play and stops the timer in the meantime ;)
        private void showHowToPlay(object sender, EventArgs e)
        {
            sessionTimer.Stop();
            MessageBox.Show(rules);
            sessionTimer.Start();
        }
        //Creates the left panel flow layout and adds the session label to it.
        private void createLeftSide()
        {
            leftPanel = new FlowLayoutPanel();
            leftPanel.FlowDirection = FlowDirection.TopDown;
            leftPanel.Size = new Size(200, this.Size.Height-80);

            leftPanel.Location = new Point(15, 15);
            leftPanel.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(leftPanel);
            leftPanel.BringToFront();

            sessionLabel = new Label();
            sessionLabel.Location = new Point(140,20);
            sessionLabel.ForeColor = Color.Purple;
            sessionLabel.AutoSize = true;
           
            Controls.Add(sessionLabel);

        }


        private void createPieces()
        {
            //Creation of buttons, rows and adding the buttons to the screen.
            for (int x = 0; x < BOARDWIDTH; x++)
            {
                for (int y = 0; y < BOARDHEIGHT; y++)
                {
                    //Create Rows and Columns for each button 
                    gameBoard.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    gameBoard.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                    pieceArray[x, y].pieceButton = new Button();
                    pieceArray[x, y].pieceButton.TabStop = false;
                    pieceArray[x, y].pieceButton.FlatStyle = FlatStyle.Flat;
                    pieceArray[x, y].pieceButton.FlatAppearance.BorderSize = 0;
                   
                    int jewelType;
                    
                     //Algorithm that makes sure there are no chains at the starting of the game
                      do
                            jewelType = random.Next(0, 7);
                        while ((x >= 2 && pieceArray[x - 1, y].jewelType == jewelType && pieceArray[x - 2, y].jewelType == jewelType)
                            ||
                                ( y >= 2 && pieceArray[x, y - 1].jewelType == jewelType && pieceArray[x, y - 2].jewelType == jewelType));
                      pieceArray[x, y].jewelType = jewelType;
                      pieceArray[x, y].pieceButton.BackgroundImage = getImage(jewelType);
                    pieceArray[x, y].pieceButton.BackgroundImageLayout = ImageLayout.Center;
                    pieceArray[x, y].pieceButton.Click += new EventHandler(this.gamePieceEvent_Click);                   
                    pieceArray[x, y].pieceButton.Size = new Size(gameBoard.Size.Width / BOARDWIDTH, gameBoard.Size.Height / BOARDHEIGHT);
                    pieceArray[x, y].pieceButton.Margin = new Padding(0);

                    //Add the Piece to the right row and column
                    gameBoard.Controls.Add(pieceArray[x, y].pieceButton, x, y);
                }
            }
        }
        //Accessses the right image for the Jewels
        private Bitmap getImage(int jewel)
        {
            switch (jewel)
            {
                case 0: return MatchThree.Properties.Resources._0;
                case 1: return MatchThree.Properties.Resources._1;
                case 2: return MatchThree.Properties.Resources._2;
                case 3: return MatchThree.Properties.Resources._3;
                case 4: return MatchThree.Properties.Resources._4;
                case 5: return MatchThree.Properties.Resources._5;
                case 6: return MatchThree.Properties.Resources._6;
                case 20: return MatchThree.Properties.Resources._20;
                default:
                    return MatchThree.Properties.Resources._20;
            }
        }
        //Initializes high score variable, label and multiplier
        private void createHighScore()
        {
            score = 0;
            multiplier = 1;
            highScoreLabel = new Label();
            highScoreLabel.TextAlign = ContentAlignment.TopLeft;
            highScoreLabel.Font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold);
            highScoreLabel.ForeColor = Color.Red;
            highScoreLabel.Size = new Size(leftPanel.Width, 50);
            highScoreLabel.Text = "Score: " + Convert.ToString(score);

            multiplierLabel = new Label();
            multiplierLabel.TextAlign = ContentAlignment.TopCenter;
            multiplierLabel.Font = new Font(FontFamily.GenericSansSerif, 15.0F, FontStyle.Bold);
            multiplierLabel.ForeColor = Color.IndianRed;
            multiplierLabel.Size = new Size(leftPanel.Width, 50);
            multiplierLabel.Text = "X" + Convert.ToString(multiplier);

            multiTimer = new System.Windows.Forms.Timer();
            multiTimer.Tick += new EventHandler(decrementMulti);
            //Multiplier goes down by 0.5 every 5 seconds. 
            multiTimer.Interval = 5000;

            leftPanel.Controls.Add(highScoreLabel);
            leftPanel.Controls.Add(multiplierLabel);

        }

   
        private void decrementMulti(object sender, EventArgs e)
        {
            if (multiplier > 1.0F)
            {
                multiplier--;
                updateLeft();
            }
            else
                multiTimer.Stop();
        }

        private void updateLeft()
        {
            multiplierLabel.Text = "X" + Convert.ToString(multiplier);
            highScoreLabel.Text = "Score: " + Convert.ToString(score);
            sessionLabel.Text = "Time Left: " + Convert.ToString(sessionTime);
            
            this.Update();

        }


        private bool checkBoardHorizontal()
        {
            int chainLength = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < BOARDHEIGHT; y++)
                {
                    chainLength = 0;
                    if (pieceArray[x, y].jewelType != 20)
                    {
                        if (pieceArray[x, y].jewelType == pieceArray[x + 1, y].jewelType && pieceArray[x, y].jewelType == pieceArray[x + 2, y].jewelType)
                        {
                            int i;
                            for (i = 3, chainLength = 2; i + x < BOARDWIDTH && pieceArray[x, y].jewelType == pieceArray[x + i, y].jewelType; i++, chainLength++);
                            score += (long)((Math.Pow(2, chainLength)) * (double)multiplier);
                            multiplier += 0.5F;
                            multiTimer.Start();
                            updateLeft();
                            sfx_Play();
                            for (int j = 0; j <= chainLength; pieceArray[x + j, y].jewelType = 20, j++) ;
                            return true; 
                        }
                    }
                    
                }
            }
            return false;
        }

        private bool checkBoardVertical()
        {
            int chainLength = 0;
            for (int x = 0; x < BOARDWIDTH; x++)
            {

                for (int y = 0; y < 8; y++)
                {
                    chainLength = 0;
                    if (pieceArray[x, y].jewelType != 20)
                    {
                        if (pieceArray[x, y].jewelType == pieceArray[x, y + 1].jewelType && pieceArray[x, y].jewelType == pieceArray[x, y + 2].jewelType)
                        {
                            int i;
                            for (i = 3, chainLength = 2; i + y < BOARDHEIGHT && pieceArray[x, y].jewelType == pieceArray[x, y + i].jewelType; i++, chainLength++) ;
                            score += (long)((Math.Pow(2,chainLength)) * (double)multiplier);
                            multiplier += 0.5F;
                            multiTimer.Start();
                            updateLeft();
                            sfx_Play();
                            for (int j = 0; j <= chainLength;pieceArray[x, y + j].jewelType = 20, j++) ;
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        private void updateBoard()
        {
            for(int i = 0; i < BOARDWIDTH; i++)
            {
                for (int j = 0; j < BOARDHEIGHT; j++)
                {

                    //pieceArray[i, j].pieceButton.BackgroundImage = Image.FromFile(@"C:\Users\arturpopov\Desktop\C#\projectSharp\Images\" + Convert.ToString(pieceArray[i, j].jewelType) + ".png");
                    pieceArray[i, j].pieceButton.BackgroundImage = getImage(pieceArray[i, j].jewelType);
                }
            }
        }

        private void fillGaps()
        {
            for (int j = 9; j > -1; j--)
            {
                for (int i = 0; i < BOARDWIDTH; i++)
                {
           
                
                    if (pieceArray[i, j].jewelType == 20)
                    {
                        
                        for(int k = j - 1; k > -1; k--)
                        {
                            if(pieceArray[i,k].jewelType != 20)
                            {
                                pieceArray[i, j].jewelType = pieceArray[i, k].jewelType;
                                pieceArray[i, k].jewelType = 20;
                                break;
                            }
                        }
                        updateBoard();
                        this.Refresh();
                        Thread.Sleep(20);
                        
                    }
                }
                //updateBoard();
                //this.Refresh();
                
                
                

                
            }
            for (int i = 0; i < BOARDWIDTH; i++)
            {
                for (int j = 9; j > -1; j--)
                {
                    if (pieceArray[i, j].jewelType == 20)
                    {
                        pieceArray[i, j].jewelType = random.Next(0, 7);
                        updateBoard();
                        this.Refresh();
                        
                      
                    }
                           
                }
            }


        }

       
        private bool checkMove(object sender)
        {
            int jewelType = ((gamePiece)sender).jewelType;
            int x = ((gamePiece)sender).pieceButton.Location.X /50;
            int y = ((gamePiece)sender).pieceButton.Location.Y /50;
            int j, i;
            int vertLength = 1;
            int horzLength = 1;
            Console.WriteLine("The X locaion: " + x + "\nAnd the Y: " + y);
            Console.WriteLine("Next CALL\n");
            for (i = x - 1; i >= 0 && pieceArray[i, y].jewelType == jewelType;  i--, horzLength++);
            for (i = x + 1; i < BOARDWIDTH && pieceArray[i, y].jewelType == jewelType; i++, horzLength++) ;
            if (horzLength >= 3) return true;
            
            for (j = y - 1; j >= 0 && pieceArray[x, j].jewelType == jewelType; j--, vertLength++);
            for (j = y + 1; j < BOARDHEIGHT && pieceArray[x, j].jewelType == jewelType; j++, vertLength++);
            return(vertLength >= 3);
        }

        private void makeMove(int x, int y, int k, int j)
        {
            gamePiece temp = new gamePiece();
            temp.pieceButton = new Button();

            temp.pieceButton.BackgroundImage = pieceArray[x, y].pieceButton.BackgroundImage;
            pieceArray[x, y].pieceButton.BackgroundImage = pieceArray[k, j].pieceButton.BackgroundImage;
            pieceArray[k, j].pieceButton.BackgroundImage = temp.pieceButton.BackgroundImage;

            temp.jewelType = pieceArray[x, y].jewelType;
            pieceArray[x, y].jewelType = pieceArray[k, j].jewelType;
            pieceArray[k, j].jewelType = temp.jewelType;
        }
        

        private void gamePieceEvent_Click(object sender, EventArgs e)
        {
            if (sessionTimer.Enabled == false)
                sessionTimer.Start();
            int x, y;
            x = (((Button)sender).Location.X) / 50;
            y = (((Button)sender).Location.Y) / 50;

            gamePiece temp = new gamePiece();
            temp.pieceButton = new Button();
            Point tempPoint = new Point(0, 0);

            pieceArray[lastEvent.X/50,lastEvent.Y/50].pieceButton.BackColor = this.BackColor;
            if ((lastEvent.X == ((Button)sender).Location.X) && (lastEvent.Y == ((((Button)sender).Location.Y)+50)))
            {
                makeMove(x, y, x, y+1);

                if (!checkMove(pieceArray[x, y]) && !checkMove(pieceArray[x, y + 1]))
                    makeMove(x, y, x, y + 1);
                else
                    lastEvent = new Point(0, 0);
            }
            else if ((lastEvent.X == ((((Button)sender).Location.X)+50)) && (lastEvent.Y == ((Button)sender).Location.Y))
            {
                makeMove(x, y, x + 1, y);

                if (!checkMove(pieceArray[x, y]) && !checkMove(pieceArray[x + 1, y]))
                    makeMove(x, y, x + 1, y);
                else
                    lastEvent = new Point(0, 0);

            }
            else if ((lastEvent.X == ((Button)sender).Location.X) && (lastEvent.Y == ((((Button)sender).Location.Y) - 50)))
            {

                makeMove(x, y, x, y - 1);
                if (!checkMove(pieceArray[x, y]) && !checkMove(pieceArray[x, y - 1]))
                    makeMove(x, y, x, y - 1);
                else
                    lastEvent = new Point(0, 0);
            }
            else if ((lastEvent.X == ((((Button)sender).Location.X) - 50)) && (lastEvent.Y == ((Button)sender).Location.Y))
            {
                makeMove(x, y, x - 1, y);
                if (!checkMove(pieceArray[x, y]) && !checkMove(pieceArray[x - 1, y]))
                    makeMove(x, y, x - 1, y);
                else
                    lastEvent = new Point(0, 0);
            }
            else
            {
                lastEvent = ((Button)sender).Location;
                ((Button)sender).BackColor = Color.Yellow;
                return;
            }
            do
            {
                fillGaps();
            }
            while (checkBoardHorizontal() || checkBoardVertical());

        }


    }
}
