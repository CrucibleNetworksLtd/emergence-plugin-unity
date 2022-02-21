using System;
using System.Collections.Generic;

namespace EmergenceEVMLocalServer.Utils
{
    public class ArgumentParser
    {

        public static Dictionary<string, string> ParseInput(string[] args)
        {
            Dictionary<string, string> parsedArgs = new Dictionary<string, string>();

            try
            {
                foreach (string v in args)
                {
                    var r = v.Split('=');

                    parsedArgs.Add(
                        r[0], r[1]
                    );
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error parsing command line arguments, please check the provided arguments");
            }

            return parsedArgs;
        }

    }
}
