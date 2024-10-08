﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Elden_Ring_Weapon_Randomizer
{
    internal class Util
    {

        public static readonly string ExeDir = Environment.CurrentDirectory;

        public static uint DeleteFromEnd(uint num, uint n)
        {
            for (int i = 1; num != 0; i++)
            {
                num = num / 10;

                if (i == n)
                    return num;
            }

            return 0;
        }

        public static string GetEmbededResource(string item)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Elden_Ring_Weapon_Randomizer.Resources.{item}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }


        public static string GetTxtResource(string filePath)
        {
            //Get local directory + file path, read file, return string contents of file

            //Path.Combine(Environment.CurrentDirectory, filePath);

            string fileString = File.ReadAllText($@"{ExeDir}/{filePath}");

            return fileString;
        }

        public static bool IsValidTxtResource(string txtLine)
        {
            //see if txt resource line is valid and should be accepted 
            //(bare bones, only checks for a couple obvious things)

            if (txtLine.Contains("//"))
            {
                txtLine = txtLine.Substring(0, txtLine.IndexOf("//")); // remove everything after "//" comments
            };
            if (string.IsNullOrWhiteSpace(txtLine) == true) //empty line check
            {
                return false; //resource line invalid
            };

            return true; //resource line valid
        }

        public static string[] RegexSplit(string source, string pattern)
        {
            return Regex.Split(source, pattern);
        }

        public static bool GetBit(byte value, int position)
        {
            if (position < 0 || position > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 7.");
            }

            return (value & (1 << position)) != 0;
        }

        public static byte SetBit(byte value, int position, bool bitValue)
        {
            if (position < 0 || position > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 7.");
            }

            if (bitValue)
            {
                return (byte)(value | (1 << position));
            }
            else
            {
                return (byte)(value & ~(1 << position));
            }
        }
    }
}
