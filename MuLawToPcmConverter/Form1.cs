using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NAudio.Codecs;
using NAudio.CoreAudioApi;
using NAudio.Dmo;
using NAudio.Dsp;
using NAudio.FileFormats;
using NAudio.Gui;
using NAudio.MediaFoundation;
using NAudio.Midi;
using NAudio.Mixer;
using NAudio.Sfz;
using NAudio.SoundFont;
using NAudio.Utils;
using NAudio.Wave;
using System.Diagnostics;//for Process() & cmd line tool
using System.Speech.AudioFormat;

namespace MuLawToPcmConverter
{
    public partial class Form1 : Form
    {
        #region Global Vars
        string inFile, outFile, outFolder;

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            if(d.ShowDialog() == DialogResult.OK)
            {
                inFile = d.FileName;
                textBox1.Text = inFile;
            }
            if (!string.IsNullOrWhiteSpace(outFolder)) generateInFilePath(outFolder);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inFile)){MessageBox.Show("select an input file first!");return;}

            FolderBrowserDialog d = new FolderBrowserDialog();
            if(d.ShowDialog() == DialogResult.OK)
            {
                outFolder = d.SelectedPath;
                generateInFilePath(outFolder);
            }
            
        }

        private void generateInFilePath(string outFolder)
        {
            //subStr inFile name & create outFile name from that
            int a = inFile.LastIndexOf("\\") + 1; //   \1\3\567.wav
            int b = inFile.LastIndexOf(".") - a; //"567"
            string outFile = inFile.Substring(a, b) + "_converted.wav";// "567_converted.wav"
                        
            textBox2.Text = outFolder + "\\" + outFile;
            outFile = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inFile) || string.IsNullOrWhiteSpace(textBox2.Text)){MessageBox.Show("select an input/output first!");return; }
            
            #region get error "NoDriver calling acmFormatSuggest": http://stackoverflow.com/questions/5652388/naudio-error-nodriver-calling-acmformatsuggest
            //using (var reader = new WaveFileReader(inFile))
            //using (var converter = WaveFormatConversionStream.CreatePcmStream(reader))
            //{
            //    WaveFileWriter.CreateWaveFile(textBox2.Text, converter);
            //}
            #endregion

            #region use command line converter : search for "Converting Audio Without ACM or MFT" : https://www.codeproject.com/Articles/501521/How-to-convert-between-most-audio-formats-in-NET
            ////var lamepath = @"C:\Users\Mark\Apps\lame.exe";
            ////var lamepath = @"C:\Users\323122960\Documents\Projects\UoT - audio files\Codec\extract msi\PlaybackInstallation\Verint\Playback\CommandLineConvertor.exe";
            //var lamepath = @"C:\Users\323122960\Documents\Projects\UoT - audio files\Codec\exe from - se442028-Logs-Yufen_SHS Log-VerintPlayback\CommandLineConvertor.exe";
            ////var lamepath = @"C:\Users\323122960\Documents\Projects\UoT - audio files\Codec\Solution_1\ffmpeg-0.5\ffmpeg-0.5\ffmpeg.exe";
            //Process p = new Process();
            //p.StartInfo.FileName = lamepath; //1st arg in cmd line, other args assigned to  p.StartInfo.Arguments ="";
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.Arguments = String.Format("-b 128 \"{0}\" \"{1}\"", inFile, outFile);
            //p.StartInfo.CreateNoWindow = true;
            //p.Start(); 
            #endregion

            #region MuLawDecoder : http://stackoverflow.com/questions/9551011/naudio-decoding-ulaw-to-pcm
            //byte[] bytes = System.IO.File.ReadAllBytes(inFile);
            //using (var writer = new WaveFileWriter(textBox2.Text, new WaveFormat(8000, 16, 1)))
            ////using(var reader = new WaveFileReader(inFile))
            ////using(byte waveByte = new WaveFileReader(inFile).ReadByte())

            //for(int i=0;i<bytes.Length;i++)
            //{
            //    short pcm = MuLawDecoder.MuLawToLinearSample(bytes[i]);
            //    writer.WriteByte
            //}
            
            #endregion

            #region get 16bit http://stackoverflow.com/questions/6647730/change-wav-file-to-16khz-and-8bit-with-using-naudio
           
            //using (var reader = new WaveFileReader(inFile))
            //{
            //    var newFormat = new WaveFormat(8000, 16, 1);
            //    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader)) //using() does the .Close() implicitly
            //    {
            //        WaveFileWriter.CreateWaveFile(outFile, conversionStream);
            //    }
            //}

            #endregion

            #region Working SOlution BUT outFile is just noise! : http://stackoverflow.com/questions/6647730/change-wav-file-to-16khz-and-8bit-with-using-naudio   ...http://stackoverflow.com/questions/6647730/change-wav-file-to-16khz-and-8bit-with-using-naudio
            ////for error "ACM format not possible"et : http://stackoverflow.com/questions/13628145/acmnotpossible-calling-acmstreamopen-naudio
            ////http://stackoverflow.com/questions/6951949/how-to-decode-rtp-packets-and-save-it-has-wav-file
            ////converted file is just a noise/sizzle : http://stackoverflow.com/questions/6647730/change-wav-file-to-16khz-and-8bit-with-using-naudio

            
            FileStream fileStream = new FileStream(inFile, FileMode.Open);
            //WaveFileReader streanSansRIFF = new WaveFileReader(fileStream);//inserted this to read ONLY actual audio part of file. STream will read even metadata RIFF header as if it was audio data
            var waveFormat = WaveFormat.CreateMuLawFormat(8000, 2);//2 is channel=steroe, 1=mono. No other value!!

            var reader = new RawSourceWaveStream(fileStream, waveFormat);

            
            using (WaveStream convertedStream = WaveFormatConversionStream.CreatePcmStream(reader))
            //using(var upsampler = new WaveFormatConversionStream(new WaveFormat(8000, 16, 1) , convertedStream))
            //using (ISampleProvider convertedStream = WaveFormatConversionStream.CreatePcmStream(reader))
            {
                //WaveFileWriter.CreateWaveFile(inFile.Replace("vox", "wav"), convertedStream);//http://stackoverflow.com/questions/6647730/change-wav-file-to-16khz-and-8bit-with-using-naudio


                WaveFileWriter.CreateWaveFile(textBox2.Text, convertedStream);
                //WaveFileWriter.CreateWaveFile16(textBox2.Text, convertedStream);//http://stackoverflow.com/questions/22869594/how-to-play-isampleprovider

            }
            fileStream.Close();
            #endregion
        }
    }
}
