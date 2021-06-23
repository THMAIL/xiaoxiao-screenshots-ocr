using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Ocr.V20181119;
using TencentCloud.Ocr.V20181119.Models;

namespace 小小截图OCR
{
    class OcrTools
    {

        private string secretId = "";
        private string secretKey = "";

        public OcrTools()
        {
            try
            {
                // 创建一个 StreamReader 的实例来读取文件 
                // using 语句也能关闭 StreamReader
                using (StreamReader sr = new StreamReader("./config.ini"))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        string[] separatingStrings = { " = " };
                        string[] configArray = line.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

                        /*foreach(var word in configArray)
                        {
                            Console.WriteLine(word);
                        }*/

                        if (configArray[0] == "SecretId")
                        {
                            secretId = configArray[1];
                        }
                        else if (configArray[0] == "SecretKey")
                        {
                            secretKey = configArray[1];
                        }
                        else
                        {
                            Console.WriteLine("配置文件格式错误");
                            System.Environment.Exit(1);
                        }
                    }
                    Console.WriteLine((secretId, secretKey));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("配置文件读取失败");
                Console.WriteLine(e.Message);
                System.Environment.Exit(1);
            }
        }

        public void GeneralBasicOCR(string imageBase64)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = secretId,
                    SecretKey = secretKey
                };

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("ocr.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;

                OcrClient client = new OcrClient(cred, "ap-shanghai", clientProfile);
                GeneralBasicOCRRequest req = new GeneralBasicOCRRequest();
                //req.ImageBase64 = base64.ImgToBase64String("C:\\Users\\lei\\Pictures\\Saved Pictures\\1 (107).jpg");
                req.ImageBase64 = imageBase64;
                Console.WriteLine(req.ImageBase64);
                GeneralBasicOCRResponse resp = client.GeneralBasicOCRSync(req);
                string result = AbstractModel.ToJsonString(resp);
                Console.WriteLine(result);

                resParse(result);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void GeneralHandwritingOCR(string imageBase64)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = secretId,
                    SecretKey = secretKey
                };

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("ocr.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;

                OcrClient client = new OcrClient(cred, "ap-shanghai", clientProfile);
                GeneralHandwritingOCRRequest req = new GeneralHandwritingOCRRequest();
                req.ImageBase64 = imageBase64;
                GeneralHandwritingOCRResponse resp = client.GeneralHandwritingOCRSync(req);

                string result = AbstractModel.ToJsonString(resp);
                Console.WriteLine(result);

                resParse(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void resParse(string result)
        {
            //解析json
            dynamic resultJson = JsonConvert.DeserializeObject(result);
            dynamic resultTextArray = resultJson.TextDetections;

            string finalResult = "";
            int latestMinY = 0, latestMaxY = 0;
            foreach (var resultText in resultTextArray)
            {
                /*//Console.WriteLine(resultText.DetectedText);
                string parag = (string)resultText.AdvancedInfo;
                //Console.WriteLine(parag.Substring(20,1));

                if (parag.Substring(20, 1) == "1" && finalResult != "")
                {
                    finalResult += "\r";
                }
                else if (parag.Substring(20, 1) != "1" && finalResult != "")
                {
                    finalResult += " ";
                }*/

                int currMinY = 999999999;//设置一个极大值
                int currMaxY = 0;
                foreach (var XY in resultText.Polygon)
                {
                    if (XY.Y < currMinY)
                    {
                        currMinY = XY.Y;
                    }

                    if (XY.Y > currMaxY)
                    {
                        currMaxY = XY.Y;
                    }
                }
                Console.WriteLine((currMinY, currMaxY));
                //检测与上一个parag是否处于一行，置信度设为90
                //if(currMinY >= latestMinY && currMinY < latestMaxY)//有处于一行的可能
                if (!(currMaxY < latestMinY || currMinY > latestMaxY))//有处于一行的可能
                {
                    //计算重合度
                    double coincidenceDegree = (double)(latestMaxY - currMinY) / (double)(currMaxY - latestMinY);
                    Console.WriteLine(("置信度为：", coincidenceDegree, resultText.DetectedText));
                    if (coincidenceDegree < 0.8)
                    {
                        //不处于同一行,换行
                        if (finalResult != "")
                        {
                            finalResult += "\r" + resultText.DetectedText;
                        }
                        else
                        {
                            finalResult += resultText.DetectedText;
                        }
                    }
                    else//处于同一行
                    {
                        finalResult += "  " + resultText.DetectedText;
                    }
                }
                else//不处于同一行，换行
                {
                    if (finalResult != "")
                    {
                        finalResult += "\r" + resultText.DetectedText;
                    }
                    else
                    {
                        finalResult += resultText.DetectedText;
                    }
                }

                //保存该行坐标信息
                latestMinY = currMinY;
                latestMaxY = currMaxY;

            }


            //Clipboard.SetText(finalResult);
            string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tempFilePath = tempPath + "\\temp.txt";
            using (StreamWriter sw = new StreamWriter(tempFilePath))
            {
                sw.Write(finalResult);
            }
            System.Diagnostics.Process.Start("notepad.exe", tempFilePath);

            //Thread.Sleep(300);

           // File.Delete(tempFilePath);
        }
    }
}
