using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.SceneManagement;
using Random = System.Random;

public enum ADDRESSFAM
{
    IPv4, IPv6
}

public static class CommonUtils
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
    
    private static string GetMd5Hash(byte[] data)
    {
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
            sBuilder.Append(data[i].ToString("x2"));
        return sBuilder.ToString();
    }

    private static bool VerifyMd5Hash(byte[] data, string hash)
    {
        return 0 == StringComparer.OrdinalIgnoreCase.Compare(GetMd5Hash(data), hash);
    }

    public static string Hash(this string data)
    {
        using (var md5 = MD5.Create())
            return GetMd5Hash(md5.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }


    public static bool IsValidEmail(this string value)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T retVal = obj.GetComponent<T>();
        if (retVal != null) return retVal;
        return obj.AddComponent<T>();
    }

    public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
    {
        for (int i = 0; i < locations.Count; i += nSize)
        {
            yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
        }
    }
    
    public static string Truncate(this string value, int maxChars)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }

    public static string MakeStandardName(this string name)
    {
        string final = name.Replace("%", "");
        return final;
    }

    public static bool Equal(this float value, float value2)
    {
        return Mathf.RoundToInt(value) == Mathf.RoundToInt(value2);
    }

    /// <summary>
    /// this fucn will remove all comma and dot char before convert it to currency
    /// many missing case in this func
    /// </summary>
    public static float GetCurrenyNumber(string input)
    {
        float retVal = 0.0f;

        int numberMinIndex = -1;
        int numberMaxIndex = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (numberMinIndex < 0 && Char.IsDigit(input[i]))
            {
                numberMinIndex = i;
            }

            if (char.IsDigit(input[i]) && i > numberMaxIndex)
            {
                numberMaxIndex = i;
            }
        }

        if (numberMinIndex == -1 || numberMaxIndex == 0) return retVal;

        string tmpStr = input.Substring(numberMinIndex, numberMaxIndex - numberMinIndex + 1);
        retVal = float.Parse(tmpStr);
        return retVal;
    }

    /// <summary>
    /// this func will not apply for firt char is 0 (ex: 0.99$) this func does not work if the currency have both (.) (,) chars
    /// many missing case in this func
    /// </summary>
    private static List<int> commaIndexs = new List<int>(5);

    public static string MultiCurrency(string input, float multi)
    {
        bool isHaveDot = false;
        if (input.Contains("."))
        {
            isHaveDot = true;
            input = input.Replace('.', ',');
        }

        string retVal = "";

        int numberMinIndex = -1;
        int numberMaxIndex = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (numberMinIndex < 0 && Char.IsDigit(input[i]))
            {
                numberMinIndex = i;
            }

            if (char.IsDigit(input[i]) && i > numberMaxIndex)
            {
                numberMaxIndex = i;
            }
        }

        if (numberMinIndex == -1 || numberMaxIndex == 0) return input;

        try
        {
            string currency = input.Substring(numberMinIndex, numberMaxIndex - numberMinIndex + 1);
            if (input[numberMinIndex].Equals('0'))
            {
                return input;
            }

            //save all the comman indexs to use later
            commaIndexs.Clear();
            for (int i = 0; i < currency.Length; i++)
            {
                if (currency[i].Equals(','))
                {
                    commaIndexs.Add(i);
                }
            }

            float currencyInNumber = float.Parse(currency);
            float finalResult = currencyInNumber * multi;
            finalResult = (int) finalResult;

            //insert back the comma
            string tmpStr = finalResult + "";
            int offsetLenght =
                (int) (Math.Floor(Math.Log10(finalResult) + 1) - Math.Floor(Math.Log10(currencyInNumber) + 1));
            //reupdate the comman index
            for (int i = 0; i < commaIndexs.Count; i++)
            {
                commaIndexs[i] += offsetLenght;
            }

            if (commaIndexs.Count == 1 && commaIndexs[0] == 0)
            {
                tmpStr = tmpStr.Insert(0, "0,");
            }
            else
            {
                int ingoreIndex = 0;
                foreach (var i in commaIndexs)
                {
                    if (i - ingoreIndex > 0)
                    {
                        tmpStr = tmpStr.Insert(i - ingoreIndex, ",");
                    }
                    else
                    {
                        ingoreIndex++;
                    }
                }
            }

            retVal = input.Replace(currency, tmpStr);
            if (isHaveDot)
            {
                retVal = retVal.Replace(',', '.');
            }

            return retVal;
        }
        catch (Exception e)
        {
            Development.Log("Cant not multi currency :" + e);
            return input;
        }
    }

    /// <summary>
    /// this func will apply for all case but have to get input price as a param
    /// </summary>
    /// <param name="input"></param>
    /// <param name="inputPrice"></param>
    /// <param name="multi"></param>
    /// <returns></returns>
    public static string MultiCurrency(string input, decimal inputPrice, float multi)
    {
        string retVal = "";

        int numberMinIndex = -1;
        int numberMaxIndex = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (numberMinIndex < 0 && Char.IsDigit(input[i]))
            {
                numberMinIndex = i;
            }

            if (char.IsDigit(input[i]) && i > numberMaxIndex)
            {
                numberMaxIndex = i;
            }
        }

        if (numberMinIndex == -1 || numberMaxIndex == 0) return input;
        string currency = input.Substring(numberMinIndex, numberMaxIndex - numberMinIndex + 1);

        float finalPrice = (float) inputPrice * multi;
        bool isCanBeInt = (inputPrice % 1) == 0;

        retVal = input.Replace(currency,
            isCanBeInt ? finalPrice.ToString("#,0") : (Math.Truncate(100 * finalPrice) / 100).ToString());
        return retVal;
    }

    public static string TrimNumber(this int num)
    {
        if (num >= 1000000000)
        {
            return (num / 1000000000).ToString("0B");
        }

        if (num >= 100000000)
        {
            return (num / 1000000D).ToString("0M");
        }

        if (num >= 1000000)
        {
            return (num / 1000000D).ToString("0M");
        }

        if (num >= 100000)
        {
            return (num / 1000D).ToString("0k");
        }

        if (num >= 10000)
        {
            return (num / 1000D).ToString("0k");
        }

        return num.ToString("#,0");
    }

    //    public static float GetNumberFromText(string str)
    //    {
    //        Regex re = new Regex(@"([0-9]+)(\d+)");
    //        Match result = re.Match(str);
    //        string value = result.Value;
    //        string alphaPart = result.Groups[1].Value;
    //        string numberPart = result.Groups[2].Value;
    //        return float.Parse(numberPart);
    //    }

    private const string IAP_PRICE_KEY = "key_iap_price_";

    /// <summary>
    /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
    /// </summary>
    /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);
 
        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }
 
    /// <summary>
    /// Determines if this RectTransform is fully visible from the specified camera.
    /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
    }
 
    /// <summary>
    /// Determines if this RectTransform is at least partially visible from the specified camera.
    /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
    }

    public static Vector3 Parabola(this Vector3 start, Vector3 end, float height, float t)
    {
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += Mathf.Sin(t * Mathf.PI) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirection);
            Vector3 up = Vector3.Cross(right, levelDirection);
//            Vector3 up = Vector3.Cross( right, travelDirection );
            if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += (Mathf.Sin(t * Mathf.PI) * height) * up.normalized;
            return result;
        }
    }
    
    public static bool DestroyComponents<T>(this GameObject obj) where  T : UnityEngine.Object
    {
        T[] listToBeDestroy = obj.GetComponentsInChildren<T>();
        try
        {
            foreach (var item in listToBeDestroy)
            {
                MonoBehaviour.Destroy(item);
            }

            return true;
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }
    
    public static bool IsCheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
    
        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));
    
            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();
    
                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);
    
                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }
    
        try
        {
            return Regex.IsMatch(email,
                                 @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                                 RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static string HideEmail(string email)
    {
        var maskedEmail = "";
        
        maskedEmail = string.Format("{0}****{1}", email[0], 
                                        email.Substring(email.IndexOf('@')-1));
        return maskedEmail;
    }
    public static long ConvertDatetimeToUnixTimeStamp(DateTime date)
    {
        var dateTimeOffset = new DateTimeOffset(date);
        var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();
        return unixDateTime;
    }
    
    public static int GetIdFromTime(DateTime date)
    {
        var dateTimeOffset = new DateTimeOffset(date);
        var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();

        int id = (int)(unixDateTime % 100000000);
        
        return id;
    }
    
    public static async void LoadSceneByName(string nameScene, bool isShowLoading)
    {
        var m_scene = SceneManager.LoadSceneAsync(nameScene);

        m_scene.allowSceneActivation = false;
        
        do
        {
            await Task.Delay(10);
        } while (m_scene.progress < 0.9f);

        await Task.Delay(100);

        m_scene.allowSceneActivation = true;
    }
    
    public static List<T> LoadAllPrefabsOfType<T>(string path) where T : MonoBehaviour
    {
#if UNITY_EDITOR
        if (path != "")
        {
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        FileInfo[] fileInf = dirInfo.GetFiles("*.prefab");

        //loop through directory loading the game object and checking if it has the component you want
        List<T> prefabComponents = new List<T>();
        foreach (FileInfo fileInfo in fileInf)
        {
            string fullPath = fileInfo.FullName.Replace(@"\","/");
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

            if(prefab!= null)
            {
                T hasT = prefab.GetComponent<T>();
                if (hasT !=null)
                {
                    prefabComponents.Add(hasT);
                }
            }
        }
        return prefabComponents;
#endif
        return null;
    }
    
    public static List<GameObject> LoadAllPrefabs(string path)
    {
#if UNITY_EDITOR
        if (path != "")
        {
            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists == false) return new List<GameObject>();
        FileInfo[] fileInf = dirInfo.GetFiles("*.prefab");

        //loop through directory loading the game object and checking if it has the component you want
        List<GameObject> prefabComponents = new List<GameObject>();
        foreach (FileInfo fileInfo in fileInf)
        {
            string fullPath = fileInfo.FullName.Replace(@"\","/");
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
            GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

            if(prefab!= null)
            {
                prefabComponents.Add(prefab);
            }
        }
        return prefabComponents;
#endif
        return new List<GameObject>();
    }
    
    private static Random rng = new Random();  
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;
        if (n <= 1) return;
        
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}

