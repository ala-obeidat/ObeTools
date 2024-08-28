using System;
using System.Text;

namespace ObeTools
{
    /// <summary>
    /// The Base1024Encoding class provides methods to encode and decode data using a custom base-1024 numeral system.
    /// This system is designed to be compatible with URL and SMS text, ensuring that the encoded strings can be safely used
    /// in these contexts without causing issues or requiring further encoding.
    /// </summary>
    public static class Base1024Encoding
    {
        static readonly Random _random = new Random();
        // The set of symbols used in the Base1024 encoding
        const string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!%,-.;_"
        + "أبتثجحخدذرزسشصضطظعغفقكلمنهويىًإآئءؤ"
        + "αβγδεζηθικλμνξοπρστυφχψω"
        + "你好世界"
        + "अनुरोधकियाआपकी"
        + "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん"
        + "가나다라마바사아자차카타파하"
        + "กขคฆงจฉชซญฎฏฐฑฒณดตถทธนปผฝพฟภมยรฤลวศษสหฬอฮ"
        + "ÁÉÍÓÚÝÞÆÖáðéíóúýþæöŐőŰű"
        + "אבגדהוזחטיךכלםמןנסעףפץצקרשת"
        + "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ"
        + "ሀሁሂሃሄህሆሇለሉሊላሌልሎሏ"
        + "অআইঈউঊঋএঐওঔকখগঘঙচছজঝঞটঠডঢণতথদধনপফবভমযরলশষসহ"
        + "ⲀⲂⲄⲆⲈⲊⲌⲐⲒⲔⲖⲘⲚⲜⲞⲠⲢⲤⲦⲨⲪⲬⲮⲰⲲⲴⲶⲸⲺⲼⲾⳀⳂⳄⳆ"
        + "ÆØÅæøåŒœŠšŽžŸÿÇçÑñĞğİıŞşßÞþÐð"
        + "ΔΘΛΞΠΣΦΨΩΑΒΓΗΙΚΜΝΟΡΤΥΧ"
        + "♠♣♥♦♪♫♯♭␣␤␥␦␧␨␩␪␫␬␭␮␯␰␱␲␳␴␵␶␷␸␹␺␻␼␽␾␿"
        + "↔↕↖↗↘↙↩↪↬↭↮↯↰↱↲↳↴↵↶↷↸↹↺↻↼↽↾↿⇀⇁⇂⇃⇄⇅⇆⇇⇈⇉⇊"
        + "≀≁≂≃≄≅≆≇≉≊≋≌≍≎≏≐≑≒≓≔≕≖≗≘≙≚≛≜≝≞≟"
        + "♨♩⚡⚢⚣⚤⚥⚦⚧⚨⚩⚪⚫⚬⚭⚮⚯⚰⚱⚲⚳⚴⚵⚶⚷⚸⚹⚺⚻⚼⚽⚾"
        + "⛂⛃⛄⛅⛈⛉⛊⛋⛌⛍⛎⛏⛐⛑⛒⛓⛔⛕⛖⛗⛘⛙⛚⛛⛜⛝⛞⛟"
        + "⬂⬃⬄⬅⬆⬇⬈⬉⬊⬋⬌⬍⬎⬏⬐⬑⬒⬓⬔⬕⬖⬗⬘⬙⬚⬛⬜⬝⬞⬟⬠⬡⬢⬣⬤⬥⬦⬧⬨⬩⬪⬫"
        + "⭅⭆⭇⭈⭉⭊⭋⭌⭍⭎⭏⭐⭑⭒⭓⭔⭕⭖⭗⭘⭙⭚⭛⭜⭝⭞⭟"
        + "⺁⺂⺃⺄⺅⺆⺇⺈⺉⺊⺋⺌⺍⺎⺏⺐⺑⺒⺓⺔⺕⺖⺗⺘⺙⺚⺛⺜⺝⺞⺟⺠⺡⺢⺣⺤⺥"
        + "⻰⻱⻲⻳⻴⻵⻶⻷⻸⻹⻺⻻⻼⻽⻾⻿⼀⼁⼂⼃⼄⼅⼆⼇⼈⼉⼊⼋⼌⼍⼎⼏⼐⼑⼒⼓⼔⼕⼖⼗⼘⼙⼚⼛⼜⼝"
        + "⼞⼟⼠⼡⼢⼣⼤⼥⼦⼧⼨⼩⼪⼫⼬⼭⼮⼯⼰⼱⼲⼳⼴⼵⼶⼷⼸⼹⼺⼻⼼⼽⼾⼿⽀⽁⽂⽃⽄⽅⽆⽇⽈⽉⽊⽋"
        + "⽌⽍⽎⽏⽐⽑⽒⽓⽔⽕⽖⽗⽘⽙⽚⽛⽜⽝⽞⽟⽠⽡⽢⽣⽤⽥⽦⽧⽨⽩⽪⽫⽬⽭⽮⽯⽰⽱⽲⽳⽴⽵⽶⽷⽸⽹⽺⽻⽼⽽⽾⽿⾀⾁⾂⾃⾄⾅⾆⾇⾈⾉⾊⾋⾌⾍⾎⾏⾐⾑⾒⾓⾔⾕⾖⾗⾘⾙⾚⾛⾜⾝⾞⾟⾠⾡⾢⾣⾤⾥⾦⾧⾨⾩⾪⾫⾬⾭⾮⾯⾰⾱⾲⾳⾴⾵⾶⾷⾸⾹⾺⾻⾼⾽⾾⾿⿀⿁⿂⿃⿄⿅⿆⿈⿉⿊⿋⿌⿍⿎⿏⿐⿑⿒⿓⿔⿕⿖⿗⿘⿙⿚⿛⿜⿝⿞⿟⿠⿡⿢⿣⿤⿥⿦⿧⿨⿩⿪⿫⿬⿭⿮⿯⿰⿱⿲⿳⿴⿵⿶⿷⿸⿹⿺⿻⿼⿽⿾⿿"
        + "ℶℷ©¶æåß";

        const string separator = "ℵℸℹ℺℻";

        // The total number of symbols in the Base1024 encoding
        private const int BaseLength = 1024;

        /// <summary>
        /// Converts a decimal number to a Base1024 encoded string.
        /// </summary>
        /// <param name="number">The decimal number to convert.</param>
        /// <returns>The Base1024 encoded string.</returns>
        public static string ToBase1024(int number)
        {
            // Handle the case where the number is 0
            if (number == 0)
            {
                return Symbols[0].ToString();
            }

            var result = new StringBuilder();

            // Convert the number to Base1024
            while (number > 0)
            {
                int remainder = number % BaseLength; // Find the remainder when dividing by 1024
                result.Insert(0, Symbols[remainder]); // Find the corresponding symbol and insert it at the beginning
                number /= BaseLength; // Reduce the number by dividing by 1024
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts a Base1024 encoded string back to a decimal number.
        /// </summary>
        /// <param name="base1024Number">The Base1024 encoded string to convert.</param>
        /// <returns>The decimal number equivalent.</returns>
        public static int FromBase1024(string base1024Number)
        {
            int result = 0;

            // Convert each character in the Base1024 string to its decimal equivalent
            foreach (char c in base1024Number)
            {
                result = result * BaseLength + Symbols.IndexOf(c); // Multiply current result by 1024 and add the index of the character
            }

            return result;
        }

        /// <summary>
        /// Converts a regular string to a Base1024 encoded string.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>The Base1024 encoded string representation of the input.</returns>
        public static string StringToBase1024(string input)
        {
            var result = new StringBuilder();

            // Convert each character in the input string to Base1024
            foreach (char c in input)
            {
                int charValue = c; // Convert the character to its numeric Unicode value
                string base1024Char = ToBase1024(charValue); // Convert the numeric value to Base1024
                result.Append(base1024Char + separator[_random.Next(0, separator.Length - 1)]); // Append the Base1024 value to the result, separated by separator
            }

            return result.ToString().Trim(); // Return the final result, trimming any trailing spaces
        }

        /// <summary>
        /// Converts a Base1024 encoded string back to a regular string.
        /// </summary>
        /// <param name="base1024Input">The Base1024 encoded string to convert.</param>
        /// <returns>The original string representation of the Base1024 input.</returns>
        public static string Base1024ToString(string base1024Input)
        {
            var result = new StringBuilder();

            // Split the Base1024 string into individual Base1024 numbers
            var base1024Chars = base1024Input.Split(separator.ToCharArray());

            // Convert each Base1024 number back to the original character
            foreach (var base1024Char in base1024Chars)
            {
                int charValue = FromBase1024(base1024Char); // Convert the Base1024 value back to its decimal equivalent
                result.Append((char)charValue); // Convert the decimal value back to a character and append to the result
            }

            return result.ToString(); // Return the final string
        }
    }

}
