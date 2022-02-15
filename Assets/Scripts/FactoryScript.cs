using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FactoryScript : MonoBehaviour
{
    [SerializeField]
    private int _idBuilding = 0;

    [SerializeField]
    private Transform[] _wayToNextBuild;

    [SerializeField]
    private GameObject _messageObject;

    [SerializeField]
    private Transform[] _consumedResourcesObject;
    [SerializeField]
    private Transform _madeResourcesObject;

    [SerializeField]
    private int[] _currentIdAcceptedResources;
    [SerializeField]
    private int _currentIdGivenResources;

    [SerializeField]
    private int _maxConsumedResources = 5;
    [SerializeField]
    private int _maxMadeResources = 5;

    private List<GameObject> _resourcesPool;

    private Animator _messageAnimator;
    private Text _messageText;

    private IEnumerator _takeAllResourcesCoroutine;
    private IEnumerator _giveAllResourcesCoroutine;

    private string[] _nameResources = { "FirstResources", "SecondResources", "ThirdResources" };

    private bool _playerInTrigger = false;
    private bool _notified = true;

    private int[] _countConsumedResources;
    private int _countMadeResources = 0;

    private float _timeToMadeResource = 0;

    private void Start()
    {
        for (int i = 0; i < _wayToNextBuild.Length; i++)
        {
            var line = Instantiate(Resources.Load<LineRenderer>("WayLine"));
            line.transform.parent = transform;

            Vector3 vec1 = _madeResourcesObject.transform.position;
            Vector3 vec2 = _wayToNextBuild[i].transform.position;

            line.GetComponent<LineRenderer>().SetPosition(0, vec1);
            line.GetComponent<LineRenderer>().SetPosition(1, vec2);
        }

        _messageAnimator = _messageObject.GetComponent<Animator>();
        _messageText = _messageObject.GetComponentInChildren<Text>();

        _resourcesPool = new List<GameObject>();
        _countConsumedResources = new int[_currentIdAcceptedResources.Length];

        _timeToMadeResource = (_currentIdAcceptedResources.Length + 1) * 1.5f;

        StartCoroutine(ProductionResources());
    }

    private void UpdateWarehouse(string typeWarehouse, int idResource, int numberConsumedResource = 0)
    {
        if (typeWarehouse == "Consumed")
        {
            AddResource(idResource, _consumedResourcesObject[numberConsumedResource]);
        }
        else if (typeWarehouse == "Made")
        {
            AddResource(idResource, _madeResourcesObject);
        }
        else if (typeWarehouse == "Given")
        {
            RemoveResource(idResource);
        }

        int currentActiveObject = 0;
        for (int i = 0; i < _consumedResourcesObject.Length; i++)
        {
            currentActiveObject = 0;
            for (int a = 0; a < _consumedResourcesObject[i].childCount; a++)
            {
                if (_consumedResourcesObject[i].GetChild(a).gameObject.activeSelf)
                {
                    _consumedResourcesObject[i].GetChild(a).localPosition = new Vector3(0, currentActiveObject * 0.2f, 0);
                    currentActiveObject++;
                }
            }
        }

        currentActiveObject = 0;
        for (int i = 0; i < _madeResourcesObject.childCount; i++)
        {
            if (_madeResourcesObject.GetChild(i).gameObject.activeSelf)
            {
                _madeResourcesObject.GetChild(i).localPosition = new Vector3(0, currentActiveObject * 0.2f, 0);
                currentActiveObject++;
            }
        }
    }

    public void AddResource(int idResource, Transform parent)
    {
        var resource = _resourcesPool.FirstOrDefault(item => item.tag.ToLower() == _nameResources[idResource].ToLower() && !item.activeSelf);
        if (resource != null)
        {
            resource.SetActive(true);
        }
        else
        {
            resource = Instantiate(Resources.Load<GameObject>(_nameResources[idResource]), parent);
            _resourcesPool.Add(resource);
        }
    }

    public void RemoveResource(int idResource)
    {
        _resourcesPool.FirstOrDefault(item => item.tag == _nameResources[idResource] && item.activeSelf)?.SetActive(false);
    }

    private IEnumerator TakeAllResources(PlayerInventoryScript playerInventoryScript)
    {
        while (_playerInTrigger)
        {
            int i = 0;
            while (i < playerInventoryScript.allResources.Length)
            {
                for (int a = 0; a < _currentIdAcceptedResources.Length; a++)
                {
                    if (_countConsumedResources[a] < _maxConsumedResources && _currentIdAcceptedResources[a] == playerInventoryScript.allResources[i])
                    {
                        _countConsumedResources[a]++;
                        playerInventoryScript.allResources[i] = 0;

                        UpdateWarehouse("Consumed", _currentIdAcceptedResources[a] - 1, a);
                        playerInventoryScript.RemoveResource(_currentIdAcceptedResources[a] - 1);

                        yield return new WaitForSeconds(0.5f);
                    }
                }

                i++;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ProductionResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            bool canMade = true;

            if (_countConsumedResources.Length > 0)
            {
                for (int i = 0; i < _countConsumedResources.Length; i++)
                {
                    if (_countConsumedResources[i] <= 0)
                    {
                        canMade = false;

                        if (!_notified)
                        {
                            _messageText.text = $"В здании №{_idBuilding} недостаточно ресурсов!";
                            _messageAnimator.SetTrigger("StartMessage");

                            StartCoroutine(HideMessage());

                            _notified = true;
                        }
                    }
                }

                if (_countMadeResources >= _maxMadeResources)
                {
                    canMade = false;

                    if (!_notified)
                    {
                        _messageText.text = $"В здании №{_idBuilding} заполнен склад!";
                        _messageAnimator.SetTrigger("StartMessage");

                        StartCoroutine(HideMessage());

                        _notified = true;
                    }
                }

                if (canMade)
                {
                    _notified = false;

                    for (int i = 0; i < _countConsumedResources.Length; i++)
                    {
                        _countConsumedResources[i]--;
                        UpdateWarehouse("Given", _currentIdAcceptedResources[i] - 1);
                    }

                    yield return new WaitForSeconds(_timeToMadeResource);

                    _countMadeResources++;

                    UpdateWarehouse("Made", _currentIdGivenResources - 1);
                }
            }
            else if (_countConsumedResources.Length == 0)
            {
                if (_countMadeResources < _maxMadeResources)
                {
                    _notified = false;

                    _countMadeResources++;

                    UpdateWarehouse("Made", _currentIdGivenResources - 1);

                    yield return new WaitForSeconds(_timeToMadeResource);
                }
                else
                {
                    if (!_notified)
                    {
                        _messageText.text = $"В здании №{_idBuilding} заполнен склад!";
                        _messageAnimator.SetTrigger("StartMessage");

                        StartCoroutine(HideMessage());

                        _notified = true;
                    }
                }
            }
        }
    }

    private IEnumerator GiveAllResources(PlayerInventoryScript playerInventoryScript)
    {
        while (_playerInTrigger)
        {
            int i = 0;
            while (i < playerInventoryScript.allResources.Length)
            {
                if (_countMadeResources > 0 && playerInventoryScript.allResources[i] == 0)
                {
                    _countMadeResources--;
                    playerInventoryScript.allResources[i] = _currentIdGivenResources;

                    UpdateWarehouse("Given", _currentIdGivenResources - 1);
                    playerInventoryScript.AddResource(_currentIdGivenResources - 1);

                    yield return new WaitForSeconds(0.5f);
                }

                i++;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(3);

        _messageAnimator.SetTrigger("EndMessage");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = true;

            _takeAllResourcesCoroutine = TakeAllResources(other.GetComponent<PlayerInventoryScript>());
            _giveAllResourcesCoroutine = GiveAllResources(other.GetComponent<PlayerInventoryScript>());

            StartCoroutine(_takeAllResourcesCoroutine);
            StartCoroutine(_giveAllResourcesCoroutine);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = false;

            StopCoroutine(_takeAllResourcesCoroutine);
            StopCoroutine(_giveAllResourcesCoroutine);
        }
    }
}