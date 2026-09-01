using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VADE.DevTools.Extensions
{
public static class ExtensionMethods
{
    public static string GetToString<T>(this List<T> alpha, string separator = " ")
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"[{alpha.Count}]");
        for (int i = 0; i < alpha.Count; i++)
        {
            stringBuilder.Append(separator);
            stringBuilder.Append(alpha[i].ToString());
        }
        return stringBuilder.ToString();
    }

    public static void Shuffle<T>(this List<T> alpha, int seed)
    {
        System.Random random = new System.Random(seed);
        for (int i = 0; i < alpha.Count; i++)
        {
            T temp = alpha[i];
            int randomIndex = random.Next(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
    }

    public static T GetClamp<T>(this List<T> lst, long index)
    {
        return lst.GetClamp((int)index);
    }

    public static T GetClamp<T>(this List<T> lst, int index)
    {
        if (lst == null || lst.Count <= 0)
        {
            return default(T);
        }
        return lst[Mathf.Clamp(index, 0, lst.Count - 1)];
    }

    public static T GetRandom<T>(this List<T> alpha)
    {
        if (alpha == null || alpha.Count == 0)
            return default(T);
        return alpha[Random.Range(0, alpha.Count)];
    }

    public static int GetRandomIndex<T>(this List<T> alpha)
    {
        if (alpha == null || alpha.Count == 0)
            return -1;
        return Random.Range(0, alpha.Count);
    }

    public static T GetRandomWithFilter<T>(this List<T> alpha, System.Predicate<T> filter)
    {
        alpha = alpha.FindAll(filter);
        if (alpha == null || alpha.Count == 0)
            return default(T);
        return alpha[Random.Range(0, alpha.Count)];
    }

    public static T GetRandom<T>(this List<T> alpha, int seed)
    {
        if (alpha == null || alpha.Count == 0)
            return default(T);
        System.Random random = new System.Random(seed);
        return alpha[random.Next(0, alpha.Count)];
    }

    public static int GetRandomIndex<T>(this List<T> alpha, int seed)
    {
        if (alpha == null || alpha.Count == 0)
            return -1;
        System.Random random = new System.Random(seed);
        return random.Next(0, alpha.Count);
    }

     public static string ToShortAmount(this long amount)
    {
        if (amount / 10000000 >= 1)
        {
            float f = amount / 10000000f;
            if (f % 1 == 0)
                return f.ToString("N0") + " Cr";
            else
                return f.ToString("N1") + " Cr";

        }
        else if (amount / 100000 >= 1)
        {
            float f = amount / 100000f;
            if (f % 1 == 0)
                return f.ToString("N0") + " L";
            else
                return f.ToString("N1") + " L";
        }
        else if (amount / 1000 >= 1)
        {
            float f = amount / 1000f;
            if (f % 1 == 0)
                return f.ToString("N0") + "K";
            else
                return f.ToString("N1") + "K";
        }
        else
        {
            return amount.ToString("N0");
        }
    }

    public static void Shuffle<T>(this List<T> alpha)
    {

        for (int i = 0; i < alpha.Count; i++)
        {
            T temp = alpha[i];
            int randomIndex = Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
    }

    public static Sprite ToSprite(this Texture2D texture)
    {
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
    }

    public static Rect GetWorldRect(this RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }

    public static string ToFirstUpper(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);

    }

    public static string ToOrdinal(this long value)
    {
        string extension = "th";
        long last_digits = value % 100;
        if (last_digits < 11 || last_digits > 13)
        {
            switch (last_digits % 10)
            {
                case 1:
                    extension = "st";
                    break;
                case 2:
                    extension = "nd";
                    break;
                case 3:
                    extension = "rd";
                    break;
            }
        }

        return extension;
    }

    public static string WrapText(this string sentence, int columnWidth)
    {

        string[] words = sentence.Split(' ');

        System.Text.StringBuilder newSentence = new System.Text.StringBuilder();

        string line = "";
        for (int i = 0; i < words.Length; i++)
        {
            if ((line + words[i]).Length > columnWidth)
            {
                newSentence.AppendLine(line);
                line = "";
            }

            line += string.Format("{0} ", words[i]);
        }

        if (line.Length > 0)
            newSentence.Append(line);

        return newSentence.ToString();
    }

    public static string TimeOnlyString(this double t, bool ignoreNegative = true)
    {
        if (t < 0 && ignoreNegative)
            t = 0;
        long sec = (long)t;
        string s = "";
        s += (Mathf.FloorToInt(sec / 60f) % 60).ToString("00") + ":";
        s += (sec % 60).ToString("00");
        return s;
    }

    public static string TimeAgo(this System.DateTime dateTime)
    {
        string result = string.Empty;
        var timeSpan = System.DateTime.Now.Subtract(dateTime);

        if (timeSpan <= System.TimeSpan.FromSeconds(60))
        {
            result = string.Format("{0} seconds ago", timeSpan.Seconds);
        }
        else if (timeSpan <= System.TimeSpan.FromMinutes(60))
        {
            result = timeSpan.Minutes > 1 ?
                string.Format("about {0} minutes ago", timeSpan.Minutes) :
                "about a minute ago";
        }
        else if (timeSpan <= System.TimeSpan.FromHours(24))
        {
            result = timeSpan.Hours > 1 ?
                string.Format("about {0} hours ago", timeSpan.Hours) :
                "about an hour ago";
        }
        else if (timeSpan <= System.TimeSpan.FromDays(30))
        {
            result = timeSpan.Days > 1 ?
                string.Format("about {0} days ago", timeSpan.Days) :
                "yesterday";
        }
        else if (timeSpan <= System.TimeSpan.FromDays(365))
        {
            result = timeSpan.Days > 30 ?
                string.Format("about {0} months ago", timeSpan.Days / 30) :
                "about a month ago";
        }
        else
        {
            result = timeSpan.Days > 365 ?
                string.Format("about {0} years ago", timeSpan.Days / 365) :
                "about a year ago";
        }

        return result;
    }

    public static void DestroyAllChild(this Transform t)
    {
        if (t != null && t.childCount > 0)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(t.GetChild(i).gameObject);
            }
        }
    }

    public static void DestroyAllChildImmediate(this Transform t)
    {
        if (t != null && t.childCount > 0)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(t.GetChild(i).gameObject);
            }
        }
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }
}
}
