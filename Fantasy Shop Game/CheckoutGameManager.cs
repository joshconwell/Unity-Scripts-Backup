using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckoutGameManager : MonoBehaviour
{
    private enum DailyModifier
    {
        NormalDay,
        AuditDay,
        CouponDay,
        CursedRush,
        ContrabandCheck,
        VolatileStock,
        PatientCrowd,
        BigSpenders
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI topBarText;
    [SerializeField] private TextMeshProUGUI customerText;
    [SerializeField] private TextMeshProUGUI customerNameText;
    [SerializeField] private Image customerPortraitImage;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TMP_InputField totalInputField;

    [Header("Customer Portraits")]
    [SerializeField] private Sprite[] customerPortraitSprites;

    [Header("End Day UI")]
    [SerializeField] private GameObject endDayPanel;
    [SerializeField] private TextMeshProUGUI endDayText;

    [Header("Spawning")]
    [SerializeField] private RectTransform counterArea;
    [SerializeField] private GameObject itemCardPrefab;
    [SerializeField] private int minItemsPerOrder = 2;
    [SerializeField] private int maxItemsPerOrder = 4;
    [SerializeField] private float nextOrderDelay = 1.5f;

    [Header("Item Loading")]
    [SerializeField] private string resourcesItemFolder = "Items";
    [SerializeField] private ItemData[] availableItems;

    [Header("Order Generation")]
    [SerializeField] private int bagCount = 3;
    [SerializeField] private int orderGenerationAttempts = 100;
    [SerializeField] private bool requireAtLeastOneLegalItem = true;

    [Header("Daily Modifiers")]
    [SerializeField] private bool enableDailyModifiers = true;

    [Header("Day Structure")]
    [SerializeField] private int customersPerDay = 3;
    [SerializeField] private int maxDay = 3;

    [Header("Customer Patience")]
    [SerializeField] private float startingPatience = 45f;
    [SerializeField] private float minimumPatience = 18f;
    [SerializeField] private float patienceLossPerCustomer = 2f;

    [Header("Reputation")]
    [SerializeField] private int startingReputation = 10;
    [SerializeField] private int maxReputation = 20;
    [SerializeField] private int reputationGainForPerfectOrder = 1;
    [SerializeField] private int reputationGainForPerfectDay = 2;

    private readonly Vector2[] spawnPositions =
    {
        new Vector2(-260f, 40f),
        new Vector2(-90f, 40f),
        new Vector2(80f, 40f),
        new Vector2(250f, 40f)
    };

    private readonly string[] customerNames =
    {
        "Sir Bristle",
        "Gob Nibblestein",
        "Mira the Mage",
        "Bonejamin",
        "Darla Doomcart",
        "Suspicious Cloak Guy",
        "Grunk the Intern",
        "Princess Taxform"
    };

    private readonly string[] customerLines =
    {
        "Please hurry. My quest party is waiting.",
        "No refunds, right? Good. I hate refunds.",
        "Some of these items may be cursed. Probably fine.",
        "I need these bagged very specifically.",
        "Do not look directly at the merchandise.",
        "I found these in a chest that screamed.",
        "My wizard said this was legal.",
        "If anything explodes, that was already like that."
    };

    private ItemCard selectedItem;

    private int day = 1;
    private int customerInDay = 1;
    private int totalCustomersServed = 0;
    private int gold = 0;
    private int reputation = 10;

    private int tipBonus = 0;
    private int disasterDamageReduction = 0;

    private int mistakesThisDay = 0;
    private bool currentOrderHadMistake = false;

    private float currentPatience;
    private string currentCustomerName;
    private Sprite currentCustomerPortrait;
    private string currentCustomerLine;

    private DailyModifier currentDailyModifier = DailyModifier.NormalDay;
    private string currentModifierName = "Normal Day";
    private string currentModifierDescription = "No special rules today.";

    private bool orderCompleted;
    private bool spawningNextOrder;
    private bool gameOver;
    private bool waitingForUpgrade;

    private void Awake()
    {
        LoadAvailableItems();
    }

    private void Start()
    {
        reputation = Mathf.Clamp(startingReputation, 0, maxReputation);

        if (endDayPanel != null)
        {
            endDayPanel.SetActive(false);
        }

        ChooseDailyModifier();
        UpdateTopBar();
        SpawnNewOrder();
    }

    private void Update()
    {
        if (gameOver || orderCompleted || spawningNextOrder || waitingForUpgrade)
        {
            return;
        }

        currentPatience -= Time.deltaTime;

        if (currentPatience <= 0f)
        {
            currentPatience = 0f;
            CustomerTimedOut();
            return;
        }

        UpdateTopBar();
        UpdateCustomerText();
    }

    private void LoadAvailableItems()
    {
        availableItems = Resources.LoadAll<ItemData>(resourcesItemFolder);

        if (availableItems == null)
        {
            availableItems = new ItemData[0];
        }

        Debug.Log("Loaded " + availableItems.Length + " item data assets from Resources/" + resourcesItemFolder);
    }

    private void ChooseDailyModifier()
    {
        if (!enableDailyModifiers || day == 1)
        {
            SetDailyModifier(DailyModifier.NormalDay);
            return;
        }

        DailyModifier[] possibleModifiers =
        {
            DailyModifier.NormalDay,
            DailyModifier.AuditDay,
            DailyModifier.CouponDay,
            DailyModifier.CursedRush,
            DailyModifier.ContrabandCheck,
            DailyModifier.VolatileStock,
            DailyModifier.PatientCrowd,
            DailyModifier.BigSpenders
        };

        DailyModifier chosenModifier = possibleModifiers[Random.Range(0, possibleModifiers.Length)];
        SetDailyModifier(chosenModifier);
    }

    private void SetDailyModifier(DailyModifier modifier)
    {
        currentDailyModifier = modifier;

        switch (currentDailyModifier)
        {
            case DailyModifier.AuditDay:
                currentModifierName = "Audit Day";
                currentModifierDescription = "Wrong totals cost 2 reputation.";
                break;

            case DailyModifier.CouponDay:
                currentModifierName = "Coupon Day";
                currentModifierDescription = "Discount and negative-value items are more common.";
                break;

            case DailyModifier.CursedRush:
                currentModifierName = "Cursed Rush";
                currentModifierDescription = "Cursed items that need their own bag are more common.";
                break;

            case DailyModifier.ContrabandCheck:
                currentModifierName = "Contraband Check";
                currentModifierDescription = "Illegal items are more common.";
                break;

            case DailyModifier.VolatileStock:
                currentModifierName = "Volatile Stock";
                currentModifierDescription = "Reactive items are more common.";
                break;

            case DailyModifier.PatientCrowd:
                currentModifierName = "Patient Crowd";
                currentModifierDescription = "Customers have +10 patience today.";
                break;

            case DailyModifier.BigSpenders:
                currentModifierName = "Big Spenders";
                currentModifierDescription = "Orders try to include 1 extra item.";
                break;

            default:
                currentModifierName = "Normal Day";
                currentModifierDescription = "No special rules today.";
                break;
        }

        Debug.Log("Day " + day + " modifier: " + currentModifierName + " — " + currentModifierDescription);
    }

    public void SelectItem(ItemCard item)
    {
        if (gameOver)
        {
            SetFeedback("Game over. Stop Play Mode to restart.");
            return;
        }

        if (waitingForUpgrade)
        {
            SetFeedback("Choose an upgrade to start the next day.");
            return;
        }

        if (orderCompleted || spawningNextOrder)
        {
            SetFeedback("Wait for the next customer.");
            return;
        }

        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }

        selectedItem = item;
        selectedItem.SetSelected(true);

        SetFeedback("Selected: " + selectedItem.ItemName);
    }

    public void ScanSelectedItem()
    {
        if (gameOver)
        {
            SetFeedback("Game over. Stop Play Mode to restart.");
            return;
        }

        if (waitingForUpgrade)
        {
            SetFeedback("Choose an upgrade to start the next day.");
            return;
        }

        if (orderCompleted || spawningNextOrder)
        {
            SetFeedback("Wait for the next customer.");
            return;
        }

        if (selectedItem == null)
        {
            SetFeedback("Select an item first.");
            return;
        }

        selectedItem.Scan();
        SetFeedback(selectedItem.GetScanText());
    }

    public void RejectSelectedItem()
    {
        if (gameOver)
        {
            SetFeedback("Game over. Stop Play Mode to restart.");
            return;
        }

        if (waitingForUpgrade)
        {
            SetFeedback("Choose an upgrade to start the next day.");
            return;
        }

        if (orderCompleted || spawningNextOrder)
        {
            SetFeedback("Wait for the next customer.");
            return;
        }

        if (selectedItem == null)
        {
            SetFeedback("Select an item first.");
            return;
        }

        if (!selectedItem.HasBeenScanned)
        {
            SetFeedback("Scan the item before rejecting it.");
            return;
        }

        if (selectedItem.IsRejected)
        {
            SetFeedback(selectedItem.ItemName + " is already rejected.");
            return;
        }

        selectedItem.RejectItem();
        SetFeedback("Rejected: " + selectedItem.ItemName + ".");
        selectedItem = null;
    }

    public void CheckoutOrder()
    {
        if (gameOver)
        {
            SetFeedback("Game over. Stop Play Mode to restart.");
            return;
        }

        if (waitingForUpgrade)
        {
            SetFeedback("Choose an upgrade to start the next day.");
            return;
        }

        if (orderCompleted || spawningNextOrder)
        {
            SetFeedback("Wait for the next customer.");
            return;
        }

        ItemCard[] items = FindObjectsOfType<ItemCard>();

        if (items.Length == 0)
        {
            SetFeedback("There are no items to checkout.");
            return;
        }

        int totalValue = 0;
        Dictionary<string, List<ItemCard>> bagItems = new Dictionary<string, List<ItemCard>>();

        foreach (ItemCard item in items)
        {
            if (!item.HasBeenScanned)
            {
                LoseReputation(1);
                SetFeedback("Mistake! You did not scan the " + item.ItemName + ".");
                return;
            }

            if (item.MustBeRejected)
            {
                if (!item.IsRejected)
                {
                    LoseReputation(2);
                    SetFeedback("Major mistake! The " + item.ItemName + " is illegal and must be rejected.");
                    return;
                }

                continue;
            }

            if (item.IsRejected)
            {
                LoseReputation(1);
                SetFeedback("Mistake! You rejected a legal item: " + item.ItemName + ".");
                return;
            }

            if (!item.IsInBag)
            {
                LoseReputation(1);
                SetFeedback("Mistake! The " + item.ItemName + " is not in a bag.");
                return;
            }

            if (!bagItems.ContainsKey(item.CurrentBagId))
            {
                bagItems[item.CurrentBagId] = new List<ItemCard>();
            }

            bagItems[item.CurrentBagId].Add(item);
            totalValue += item.BaseValue;
        }

        foreach (KeyValuePair<string, List<ItemCard>> bag in bagItems)
        {
            List<ItemCard> itemsInBag = bag.Value;

            foreach (ItemCard item in itemsInBag)
            {
                if (item.MustBeBaggedAlone && itemsInBag.Count > 1)
                {
                    LoseReputation(2);
                    SetFeedback("Major mistake! The " + item.ItemName + " must be bagged alone.");
                    return;
                }
            }
        }

        foreach (KeyValuePair<string, List<ItemCard>> bag in bagItems)
        {
            List<ItemCard> itemsInBag = bag.Value;

            for (int i = 0; i < itemsInBag.Count; i++)
            {
                for (int j = i + 1; j < itemsInBag.Count; j++)
                {
                    ItemCard firstItem = itemsInBag[i];
                    ItemCard secondItem = itemsInBag[j];

                    if (firstItem.ExplodesWith(secondItem) || secondItem.ExplodesWith(firstItem))
                    {
                        int damage = Mathf.Max(1, 3 - disasterDamageReduction);
                        LoseReputation(damage);
                        SetFeedback("Disaster! " + firstItem.ItemName + " reacted with " + secondItem.ItemName + " in " + bag.Key + ". -" + damage + " reputation.");
                        return;
                    }
                }
            }
        }

        if (!PlayerEnteredCorrectTotal(totalValue))
        {
            return;
        }

        int payout = totalValue + tipBonus;

        gold += payout;
        totalCustomersServed++;

        bool gainedReputation = false;

        if (!currentOrderHadMistake)
        {
            GainReputation(reputationGainForPerfectOrder);
            gainedReputation = reputationGainForPerfectOrder > 0;
        }

        orderCompleted = true;

        UpdateTopBar();

        string message = "Correct! " + currentCustomerName + " paid " + totalValue + " gold.";

        if (tipBonus > 0)
        {
            message = "Correct! " + currentCustomerName + " paid " + totalValue + " gold, plus " + tipBonus + " tip.";
        }

        if (gainedReputation)
        {
            message += " +" + reputationGainForPerfectOrder + " reputation.";
        }

        SetFeedback(message);

        StartCoroutine(AdvanceAfterCustomer());
    }

    private bool PlayerEnteredCorrectTotal(int correctTotal)
    {
        if (totalInputField == null)
        {
            SetFeedback("Missing Total Input Field reference on GameManager.");
            return false;
        }

        string rawInput = totalInputField.text.Trim();

        if (string.IsNullOrEmpty(rawInput))
        {
            SetFeedback("Enter the total gold amount before checkout.");
            return false;
        }

        if (!int.TryParse(rawInput, out int enteredTotal))
        {
            SetFeedback("Enter a valid number for the total.");
            return false;
        }

        if (enteredTotal != correctTotal)
        {
            int penalty = GetWrongTotalPenalty();
            LoseReputation(penalty);
            SetFeedback("Wrong total! You entered " + enteredTotal + "g, but the order total was " + correctTotal + "g. -" + penalty + " reputation.");
            return false;
        }

        return true;
    }

    private int GetWrongTotalPenalty()
    {
        if (currentDailyModifier == DailyModifier.AuditDay)
        {
            return 2;
        }

        return 1;
    }

    private void CustomerTimedOut()
    {
        if (gameOver || orderCompleted || spawningNextOrder || waitingForUpgrade)
        {
            return;
        }

        orderCompleted = true;
        LoseReputation(2);

        if (!gameOver)
        {
            SetFeedback(currentCustomerName + " got tired of waiting and left! -2 reputation.");
            StartCoroutine(AdvanceAfterCustomer());
        }
    }

    private IEnumerator AdvanceAfterCustomer()
    {
        spawningNextOrder = true;

        yield return new WaitForSeconds(nextOrderDelay);

        if (gameOver)
        {
            yield break;
        }

        ClearCurrentOrder();

        if (customerInDay >= customersPerDay)
        {
            EndDay();
        }
        else
        {
            customerInDay++;
            SpawnNewOrder();
            spawningNextOrder = false;
        }
    }

    private void EndDay()
    {
        spawningNextOrder = false;
        waitingForUpgrade = true;
        orderCompleted = true;

        ClearTotalInput();

        bool earnedPerfectDayBonus = mistakesThisDay == 0;

        if (earnedPerfectDayBonus)
        {
            GainReputation(reputationGainForPerfectDay);
        }

        UpdateTopBar();

        string dayPerformanceText = earnedPerfectDayBonus
            ? "Perfect Day Bonus: +" + reputationGainForPerfectDay + " reputation"
            : "Mistakes Today: " + mistakesThisDay;

        if (day >= maxDay)
        {
            gameOver = true;

            if (endDayPanel != null)
            {
                endDayPanel.SetActive(true);
            }

            if (endDayText != null)
            {
                endDayText.text =
                    "Run Complete!" +
                    "\n\nYou survived " + maxDay + " days." +
                    "\nGold earned: " + gold +
                    "\nReputation: " + reputation + "/" + maxReputation +
                    "\n" + dayPerformanceText;
            }

            SetFeedback("You survived the week! Prototype win.");
            UpdateCustomerText();
            return;
        }

        if (endDayPanel != null)
        {
            endDayPanel.SetActive(true);
        }

        if (endDayText != null)
        {
            endDayText.text =
                "Day " + day + " Complete!" +
                "\n\nToday's Modifier: " + currentModifierName +
                "\n" + currentModifierDescription +
                "\n\nGold: " + gold +
                "\nReputation: " + reputation + "/" + maxReputation +
                "\n" + dayPerformanceText +
                "\n\nChoose an upgrade.";
        }

        SetFeedback("Day complete. Choose an upgrade.");
        UpdateCustomerText();
    }

    public void ChooseUpgradeCustomerBell()
    {
        if (!waitingForUpgrade || gameOver)
        {
            return;
        }

        startingPatience += 5f;
        StartNextDay("Customer Bell installed. Customers have more patience.");
    }

    public void ChooseUpgradeTipJar()
    {
        if (!waitingForUpgrade || gameOver)
        {
            return;
        }

        tipBonus += 2;
        StartNextDay("Tip Jar upgraded. Correct orders earn +" + tipBonus + " bonus gold.");
    }

    public void ChooseUpgradeInsurance()
    {
        if (!waitingForUpgrade || gameOver)
        {
            return;
        }

        disasterDamageReduction += 1;
        StartNextDay("Insurance purchased. Disasters hurt less.");
    }

    private void StartNextDay(string upgradeMessage)
    {
        day++;
        customerInDay = 1;
        mistakesThisDay = 0;

        waitingForUpgrade = false;
        orderCompleted = false;
        spawningNextOrder = false;

        ChooseDailyModifier();

        if (endDayPanel != null)
        {
            endDayPanel.SetActive(false);
        }

        SpawnNewOrder();
        SetFeedback(upgradeMessage + " Day " + day + " begins. Modifier: " + currentModifierName + ".");
    }

    private void SpawnNewOrder()
    {
        orderCompleted = false;
        selectedItem = null;
        currentOrderHadMistake = false;

        ClearTotalInput();

        currentPatience = Mathf.Max(
            minimumPatience,
            startingPatience - ((day - 1) * 3f) - ((customerInDay - 1) * patienceLossPerCustomer)
        );

        if (currentDailyModifier == DailyModifier.PatientCrowd)
        {
            currentPatience += 10f;
        }

        int customerIndex = Random.Range(0, customerNames.Length);
        currentCustomerName = customerNames[customerIndex];
        currentCustomerPortrait = GetCustomerPortrait(customerIndex);
        currentCustomerLine = customerLines[Random.Range(0, customerLines.Length)];

        if (counterArea == null)
        {
            SetFeedback("Missing Counter Area reference on GameManager.");
            return;
        }

        if (itemCardPrefab == null)
        {
            SetFeedback("Missing Item Card Prefab reference on GameManager.");
            return;
        }

        List<ItemData> eligibleItems = GetEligibleItemsForCurrentDay();

        if (eligibleItems.Count == 0)
        {
            SetFeedback("No eligible items found for Day " + day + ". Check item minDay values.");
            return;
        }

        int itemCount = Random.Range(minItemsPerOrder, maxItemsPerOrder + 1);

        if (currentDailyModifier == DailyModifier.BigSpenders)
        {
            itemCount += 1;
        }

        itemCount = Mathf.Clamp(itemCount, 1, spawnPositions.Length);
        itemCount = Mathf.Min(itemCount, eligibleItems.Count);

        List<ItemData> orderItems = GenerateSolvableOrder(itemCount, eligibleItems);

        if (orderItems.Count == 0)
        {
            SetFeedback("Could not generate a solvable order. Check item rules.");
            return;
        }

        for (int i = 0; i < orderItems.Count; i++)
        {
            ItemData chosenItemData = orderItems[i];

            GameObject spawnedItem = Instantiate(itemCardPrefab, counterArea);
            spawnedItem.name = chosenItemData.itemName + " Card";

            RectTransform itemRect = spawnedItem.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.anchoredPosition = spawnPositions[i];

            ItemCard itemCard = spawnedItem.GetComponent<ItemCard>();

            if (itemCard != null)
            {
                itemCard.SetData(chosenItemData);
            }
        }

        Debug.Log("Day " + day + " eligible item count: " + eligibleItems.Count + ". Modifier: " + currentModifierName);

        UpdateTopBar();
        UpdateCustomerText();
        SetFeedback("Day " + day + ", Customer " + customerInDay + ": " + currentModifierName + ". Scan, reject, bag, total, checkout.");
    }

    private Sprite GetCustomerPortrait(int customerIndex)
    {
        if (customerPortraitSprites == null || customerPortraitSprites.Length == 0)
        {
            return null;
        }

        if (customerIndex < 0 || customerIndex >= customerPortraitSprites.Length)
        {
            return null;
        }

        return customerPortraitSprites[customerIndex];
    }

    private List<ItemData> GetEligibleItemsForCurrentDay()
    {
        List<ItemData> eligibleItems = new List<ItemData>();

        foreach (ItemData item in availableItems)
        {
            if (item == null)
            {
                continue;
            }

            if (item.minDay <= day)
            {
                eligibleItems.Add(item);
            }
        }

        return eligibleItems;
    }

    private List<ItemData> GenerateSolvableOrder(int itemCount, List<ItemData> eligibleItems)
    {
        for (int attempt = 0; attempt < orderGenerationAttempts; attempt++)
        {
            List<ItemData> candidateOrder = PickWeightedUniqueItems(itemCount, eligibleItems);

            if (candidateOrder.Count == 0)
            {
                continue;
            }

            if (IsOrderSolvable(candidateOrder))
            {
                return candidateOrder;
            }
        }

        Debug.LogWarning("Failed to generate a solvable order after " + orderGenerationAttempts + " attempts. Falling back to safe legal items.");

        return GenerateFallbackSafeOrder(itemCount, eligibleItems);
    }

    private List<ItemData> PickWeightedUniqueItems(int itemCount, List<ItemData> sourcePool)
    {
        List<ItemData> pool = new List<ItemData>(sourcePool);
        List<ItemData> pickedItems = new List<ItemData>();

        while (pickedItems.Count < itemCount && pool.Count > 0)
        {
            int totalWeight = 0;

            foreach (ItemData item in pool)
            {
                totalWeight += GetItemWeightForCurrentModifier(item);
            }

            if (totalWeight <= 0)
            {
                break;
            }

            int roll = Random.Range(0, totalWeight);
            int runningTotal = 0;
            int chosenIndex = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                runningTotal += GetItemWeightForCurrentModifier(pool[i]);

                if (roll < runningTotal)
                {
                    chosenIndex = i;
                    break;
                }
            }

            ItemData pickedItem = pool[chosenIndex];
            pool.RemoveAt(chosenIndex);

            if (pickedItem == null)
            {
                continue;
            }

            pickedItems.Add(pickedItem);
        }

        return pickedItems;
    }

    private int GetItemWeightForCurrentModifier(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        int weight = 10;

        switch (currentDailyModifier)
        {
            case DailyModifier.CouponDay:
                if (item.baseValue < 0)
                {
                    weight += 40;
                }
                break;

            case DailyModifier.CursedRush:
                if (item.mustBeBaggedAlone)
                {
                    weight += 35;
                }
                break;

            case DailyModifier.ContrabandCheck:
                if (item.mustBeRejected)
                {
                    weight += 35;
                }
                break;

            case DailyModifier.VolatileStock:
                if (item.explodesWithItemIdsInSameBag != null && item.explodesWithItemIdsInSameBag.Length > 0)
                {
                    weight += 35;
                }
                break;
        }

        return weight;
    }

    private List<ItemData> GenerateFallbackSafeOrder(int itemCount, List<ItemData> eligibleItems)
    {
        List<ItemData> safeItems = new List<ItemData>();

        foreach (ItemData item in eligibleItems)
        {
            if (item == null)
            {
                continue;
            }

            bool hasExplosiveRules = item.explodesWithItemIdsInSameBag != null && item.explodesWithItemIdsInSameBag.Length > 0;

            if (!item.mustBeRejected && !item.mustBeBaggedAlone && !hasExplosiveRules)
            {
                safeItems.Add(item);
            }
        }

        List<ItemData> fallbackOrder = new List<ItemData>();

        while (fallbackOrder.Count < itemCount && safeItems.Count > 0)
        {
            int randomIndex = Random.Range(0, safeItems.Count);
            fallbackOrder.Add(safeItems[randomIndex]);
            safeItems.RemoveAt(randomIndex);
        }

        return fallbackOrder;
    }

    private bool IsOrderSolvable(List<ItemData> orderItems)
    {
        List<ItemData> legalItems = new List<ItemData>();

        foreach (ItemData item in orderItems)
        {
            if (item == null)
            {
                return false;
            }

            if (!item.mustBeRejected)
            {
                legalItems.Add(item);
            }
        }

        if (requireAtLeastOneLegalItem && legalItems.Count == 0)
        {
            return false;
        }

        if (legalItems.Count == 0)
        {
            return true;
        }

        int[] bagAssignments = new int[legalItems.Count];

        for (int i = 0; i < bagAssignments.Length; i++)
        {
            bagAssignments[i] = -1;
        }

        return TryAssignItemToBag(legalItems, bagAssignments, 0);
    }

    private bool TryAssignItemToBag(List<ItemData> legalItems, int[] bagAssignments, int itemIndex)
    {
        if (itemIndex >= legalItems.Count)
        {
            return true;
        }

        for (int bagIndex = 0; bagIndex < bagCount; bagIndex++)
        {
            bagAssignments[itemIndex] = bagIndex;

            if (CurrentBagAssignmentsAreValid(legalItems, bagAssignments, itemIndex))
            {
                if (TryAssignItemToBag(legalItems, bagAssignments, itemIndex + 1))
                {
                    return true;
                }
            }

            bagAssignments[itemIndex] = -1;
        }

        return false;
    }

    private bool CurrentBagAssignmentsAreValid(List<ItemData> legalItems, int[] bagAssignments, int highestAssignedIndex)
    {
        for (int i = 0; i <= highestAssignedIndex; i++)
        {
            if (bagAssignments[i] < 0)
            {
                continue;
            }

            ItemData firstItem = legalItems[i];

            for (int j = i + 1; j <= highestAssignedIndex; j++)
            {
                if (bagAssignments[j] < 0)
                {
                    continue;
                }

                if (bagAssignments[i] != bagAssignments[j])
                {
                    continue;
                }

                ItemData secondItem = legalItems[j];

                if (firstItem.mustBeBaggedAlone || secondItem.mustBeBaggedAlone)
                {
                    return false;
                }

                if (ItemDataExplodesWith(firstItem, secondItem) || ItemDataExplodesWith(secondItem, firstItem))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool ItemDataExplodesWith(ItemData firstItem, ItemData secondItem)
    {
        if (firstItem == null || secondItem == null || firstItem.explodesWithItemIdsInSameBag == null)
        {
            return false;
        }

        foreach (string badItemId in firstItem.explodesWithItemIdsInSameBag)
        {
            if (secondItem.itemId == badItemId)
            {
                return true;
            }
        }

        return false;
    }

    private void ClearCurrentOrder()
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
            selectedItem = null;
        }

        ItemCard[] items = FindObjectsOfType<ItemCard>();

        foreach (ItemCard item in items)
        {
            Destroy(item.gameObject);
        }

        ClearTotalInput();
    }

    private void ClearTotalInput()
    {
        if (totalInputField != null)
        {
            totalInputField.text = "";
        }
    }

    private void LoseReputation(int amount)
    {
        currentOrderHadMistake = true;
        mistakesThisDay++;

        reputation -= amount;

        if (reputation < 0)
        {
            reputation = 0;
        }

        UpdateTopBar();

        if (reputation <= 0)
        {
            gameOver = true;
            orderCompleted = true;
            spawningNextOrder = false;
            waitingForUpgrade = false;
            ClearCurrentOrder();

            if (endDayPanel != null)
            {
                endDayPanel.SetActive(false);
            }

            SetFeedback("Game over! Your reputation hit 0.");
            UpdateCustomerText();
        }
    }

    private void GainReputation(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        reputation += amount;

        if (reputation > maxReputation)
        {
            reputation = maxReputation;
        }

        UpdateTopBar();
    }

    private void UpdateTopBar()
    {
        if (topBarText != null)
        {
            int displayedTime = Mathf.CeilToInt(currentPatience);

            topBarText.text =
                "Day " + day +
                " | " + currentModifierName +
                " | Customer " + customerInDay + "/" + customersPerDay +
                " | Time: " + displayedTime +
                " | Gold: " + gold +
                " | Reputation: " + reputation + "/" + maxReputation;
        }
    }

    private void UpdateCustomerText()
    {
        if (gameOver && day >= maxDay && reputation > 0)
        {
            SetCustomerDisplay("Run Complete", "You survived the week.", null);
            return;
        }

        if (gameOver)
        {
            SetCustomerDisplay("Shop Closed", "Your reputation hit zero.", null);
            return;
        }

        if (waitingForUpgrade)
        {
            SetCustomerDisplay("End of Day", "Choose an upgrade to continue.", null);
            return;
        }

        string speech =
            "\"" + currentCustomerLine + "\"" +
            "\n\nModifier: " + currentModifierName +
            "\n" + currentModifierDescription +
            "\n\nPatience: " + Mathf.CeilToInt(currentPatience);

        SetCustomerDisplay(currentCustomerName, speech, currentCustomerPortrait);
    }

    private void SetCustomerDisplay(string displayName, string speech, Sprite portrait)
    {
        if (customerNameText != null)
        {
            customerNameText.text = displayName;
        }

        if (customerText != null)
        {
            customerText.text = speech;
        }

        if (customerPortraitImage != null)
        {
            customerPortraitImage.sprite = portrait;
            customerPortraitImage.enabled = portrait != null;
        }
    }

    public void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}