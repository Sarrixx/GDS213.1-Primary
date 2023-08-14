using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [System.Serializable]
    private struct DayData
    {
        [SerializeField] private int day;
        [SerializeField] private int buildIndex;
        [SerializeField] private float loadDelay;

        public int Day { get { return day; } }
        public int BuildIndex { get { return buildIndex; } }
        public float LoadDelay { get { return loadDelay; } }
    }

    [SerializeField] private Text dayText;
    [SerializeField] private DayData[] days;

    private Coroutine loadingRoutine;

    public int DayIndex { get; private set; } = 0;
    public static DayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GameInitialisation());
    }

    public int TriggerNextDay()
    {
        if(loadingRoutine == null)
        {
            DayIndex++;
            if (DayIndex < days.Length)
            {
                loadingRoutine = StartCoroutine(LoadNextDay());
                return 1;
            }
            else
            {
                loadingRoutine = StartCoroutine(LoadMenu());
                return 0;
            }
        }
        return -1;
    }

    IEnumerator GameInitialisation()
    {
        dayText.text = $"DAY {days[DayIndex].Day}";
        dayText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        dayText.transform.parent.gameObject.SetActive(false);
        DialogueManager.Instance.TriggerDayStart();
        PlayerController.Instance.MovementEnabled = true;
        MouseLook.Instance.LookEnabled = true;
    }

    IEnumerator LoadNextDay()
    {
        yield return new WaitForSeconds(days[DayIndex].LoadDelay);
        dayText.text = $"DAY {days[DayIndex].Day}";
        dayText.transform.parent.gameObject.SetActive(true);
        if (days[DayIndex].BuildIndex >= 0)
        {
            float start = Time.time;
            var loading = SceneManager.LoadSceneAsync(days[DayIndex].BuildIndex, LoadSceneMode.Single);
            while (loading.isDone == false)
            {
                yield return null;
            }
            float end = Time.time;
            if (end - start < 3)
            {
                yield return new WaitForSeconds(3 - (end - start));
            }
        }
        dayText.transform.parent.gameObject.SetActive(false);
        loadingRoutine = null;
        DialogueManager.Instance.TriggerDayStart();
        PlayerController.Instance.MovementEnabled = true;
        MouseLook.Instance.LookEnabled = true;
    }

    IEnumerator LoadMenu()
    {
        dayText.text = $"";
        dayText.transform.parent.gameObject.SetActive(true);
        float start = Time.time;
        var loading = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        while (loading.isDone == false)
        {
            yield return null;
        }
        float end = Time.time;
        if (end - start < 3)
        {
            yield return new WaitForSeconds(3 - (end - start));
        }
        dayText.transform.parent.gameObject.SetActive(false);
        loadingRoutine = null;
        Destroy(gameObject);
    }
}
