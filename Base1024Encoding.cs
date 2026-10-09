using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObeTools
{
    /// <summary>
    /// The Base1024Encoding class provides methods to encode and decode data using a custom base-1024 numeral system
    /// (1,024 distinct characters, one UTF-16 code unit each).
    /// </summary>
    /// <remarks>
    /// The output is NOT URL-safe as is ('%', ',', ';' and non-ASCII characters need URL encoding), and any non-GSM
    /// character makes an SMS use UCS-2 (70 characters per message). It is compact counted in characters, not in bytes.
    /// </remarks>
    public static class Base1024Encoding
    {
        [ThreadStatic]
        private static Random _random;
        // The set of symbols used in the Base1024 encoding
        const string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!%,-.;_"
        + "أبتثجحخدذرزسشصضطظعغفقكلمنهويىًإآئءؤ"
        + "αβγδεζηθικλμνξοπρστυφχψω"
        + "你好世界"
        + "अनुरोधकियाआपĀी"
        + "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん"
        + "가나다라마바사아자차카타파하"
        + "กขคฆงจฉชซญฎฏฐฑฒณดตถทธนปผฝพฟภมยรฤลวศษสหฬอฮ"
        + "ÁÉÍÓÚÝÞÆÖáðéíóúýþæöŐőŰű"
        + "אבגדהוזחטיךכלםמןנסעףפץצקרשת"
        + "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ"
        + "ሀሁሂሃሄህሆሇለሉሊላሌልሎሏ"
        + "অআইঈউঊঋএঐওঔকখগঘঙচছজঝঞটঠডঢণতথদধনপফবভমযরলশষসহ"
        + "ⲀⲂⲄⲆⲈⲊⲌⲐⲒⲔⲖⲘⲚⲜⲞⲠⲢⲤⲦⲨⲪⲬⲮⲰⲲⲴⲶⲸⲺⲼⲾⳀⳂⳄⳆ"
        + "āØÅĒøåŒœŠšŽžŸÿÇçÑñĞğİıŞşßēĪÐī"
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
        + "ℶℷ©¶Ōō";

        const string separator = "ℵℸℹ℺℻";

        // The total number of symbols in the Base1024 encoding
        private const int BaseLength = 1024;

        static Base1024Encoding()
        {
            var distinct = new HashSet<char>(Symbols);
            if (Symbols.Length != BaseLength || distinct.Count != BaseLength || separator.Any(distinct.Contains))
            {
                throw new InvalidOperationException("Base1024 symbol table must hold exactly 1024 distinct characters and no separator.");
            }
        }

        /// <summary>
        /// Converts a decimal number to a Base1024 encoded string.
        /// </summary>
        /// <param name="number">The decimal number to convert (0 or more).</param>
        /// <returns>The Base1024 encoded string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The number is negative.</exception>
        public static string ToBase1024(int number)
        {
            if (number < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, "Only numbers of 0 or more can be encoded.");
            }

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
        /// <exception cref="FormatException">The string contains a character that is not a Base1024 symbol.</exception>
        /// <exception cref="OverflowException">The value does not fit in an int.</exception>
        public static int FromBase1024(string base1024Number)
        {
            if (base1024Number == null)
            {
                throw new ArgumentNullException(nameof(base1024Number));
            }

            int result = 0;

            // Convert each character in the Base1024 string to its decimal equivalent
            foreach (char c in base1024Number)
            {
                int digit = Symbols.IndexOf(c);
                if (digit < 0)
                {
                    throw new FormatException($"'{c}' is not a Base1024 symbol.");
                }
                result = checked((result * BaseLength) + digit); // Multiply current result by 1024 and add the index of the character
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
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var random = _random ??= new Random(Guid.NewGuid().GetHashCode());
            var result = new StringBuilder();

            // Convert each character in the input string to Base1024, separated by a random separator
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(separator[random.Next(0, separator.Length)]);
                }
                result.Append(ToBase1024(input[i])); // the numeric Unicode value of the character
            }

            return result.ToString();
        }

        /// <summary>
        /// Converts a Base1024 encoded string back to a regular string.
        /// Strings from older versions (with a trailing separator) decode the same, without an extra '\0'.
        /// </summary>
        /// <param name="base1024Input">The Base1024 encoded string to convert.</param>
        /// <returns>The original string representation of the Base1024 input.</returns>
        public static string Base1024ToString(string base1024Input)
        {
            if (base1024Input == null)
            {
                throw new ArgumentNullException(nameof(base1024Input));
            }

            var result = new StringBuilder();

            // Split the Base1024 string into individual Base1024 numbers
            var base1024Chars = base1024Input.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // Convert each Base1024 number back to the original character
            foreach (var base1024Char in base1024Chars)
            {
                result.Append(checked((char)FromBase1024(base1024Char)));
            }

            return result.ToString(); // Return the final string
        }
    }

}
