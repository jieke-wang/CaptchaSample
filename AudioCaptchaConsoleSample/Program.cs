using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AudioCaptchaConsoleSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            await DemoAsync();
        }

        static async Task DemoAsync()
        {
            var config = SpeechConfig.FromSubscription("YourSubscriptionKey", "YourServiceRegion");
            //{
            //    using var audioConfig = AudioConfig.FromWavFileOutput("path/to/write/file.wav");
            //    using var synthesizer = new SpeechSynthesizer(config, audioConfig);
            //    await synthesizer.SpeakTextAsync("A simple test to write to a file.");
            //}
            {
                using var synthesizer = new SpeechSynthesizer(config);
                await synthesizer.SpeakTextAsync("Synthesizing directly to speaker output.");
            }
            //{
            //    using var synthesizer = new SpeechSynthesizer(config, null);

            //    var result = await synthesizer.SpeakTextAsync("Getting the response as an in-memory stream.");
            //    using var stream = AudioDataStream.FromResult(result);
            //}

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

// https://msdn.microsoft.com/zh-cn/library/system.speech.synthesis.speechsynthesizer(v=vs.110).aspx
// http://www.cnblogs.com/yincheng01/archive/2009/09/09/2213317.html
// https://docs.microsoft.com/en-us/answers/questions/396385/local-text-to-speech-tts-net-core.html
// C# TTS-文本转语音 https://www.cnblogs.com/webenh/p/12432117.html
// https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/get-started-text-to-speech?tabs=script%2Cwindowsinstall&pivots=programming-language-csharp
