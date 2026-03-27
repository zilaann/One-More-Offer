using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class EconomyManager : MonoBehaviour
{
    [Header("Oyuncu Verileri")]
    public int playerMoney = 1000;
    private bool hasItem = false;
    private string currentItemName = "";
    private int onayliTeklif = 0;
    private int uretilenMusteriSayisi = 0; // İLK MÜŞTERİ KONTROLÜ İÇİN SAYAÇ

    [Header("UI Elemanları")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI itemInfoText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI customerText;
    public TextMeshProUGUI mesajText;
    public Slider sabirSlider;
    public TMP_InputField teklifInput;
    public Button satinAlButonu;
    public Button satButonu;

    private int esyaninGercekDegeri;
    private string[] itemNames = { "Antika Kılıç", "Gizemli Elmas", "Eski Harita", "Ejderha Pulu", "Altın Kupa", "Büyülü İksir" };
    private string[] customerNames = { "Sercan", "Kerem", "Pelin", "Can", "Zeynep", "Mert", "Aslı" };

    private Queue<CustomerData> musteriSirasi = new Queue<CustomerData>();
    private CustomerData mevcutMusteri;
    private bool dükkanDolu = false;

    [System.Serializable]
    public class CustomerData
    {
        public string isim;
        public bool alici;
        public float sabir;
    }

    void Start()
    {
        // İlk başta 3 müşteri üretilir (İlki garantili satıcı olacak)
        for (int i = 0; i < 3; i++) YeniMusteriUret();
        GenerateNewMarketItem();
        SiradakiMusteriyiCagir();
        UpdateUI();
    }

    void Update()
    {
        if (dükkanDolu && mevcutMusteri != null)
        {
            mevcutMusteri.sabir -= Time.deltaTime * 1.5f;
            UpdateUI();
            if (mevcutMusteri.sabir <= 0) SiradakiMusteriyiCagir();
        }
    }

    void YeniMusteriUret()
    {
        CustomerData yeni = new CustomerData();
        yeni.isim = customerNames[Random.Range(0, customerNames.Length)];

        // KRİTİK MANTIK: Eğer ilk üretilen müşteriyse Alıcı olamaz (false).
        if (uretilenMusteriSayisi == 0)
        {
            yeni.alici = false;
        }
        else
        {
            yeni.alici = (Random.value > 0.5f);
        }

        yeni.sabir = 100f;
        musteriSirasi.Enqueue(yeni);
        uretilenMusteriSayisi++; // Her üretimde sayacı artır
    }

    public void SiradakiMusteriyiCagir()
    {
        if (musteriSirasi.Count > 0)
        {
            mevcutMusteri = musteriSirasi.Dequeue();
            dükkanDolu = true;
            onayliTeklif = 0;
            mesajText.text = "Hoş geldiniz! Teklifinizi bekliyorum.";
            YeniMusteriUret();

            // Eğer elimiz boşsa yeni eşya üretilir
            if (!hasItem) GenerateNewMarketItem();

            teklifInput.text = "";
            UpdateUI();
        }
    }

    void GenerateNewMarketItem()
    {
        currentItemName = itemNames[Random.Range(0, itemNames.Length)];
        esyaninGercekDegeri = Random.Range(300, 1000);
    }

    public void TeklifVer()
    {
        if (!dükkanDolu || string.IsNullOrEmpty(teklifInput.text)) return;

        int teklif = int.Parse(teklifInput.text);
        float farkYuzdesi = (float)teklif / esyaninGercekDegeri;

        if (mevcutMusteri.alici)
        {
            if (farkYuzdesi <= 1.25f)
            {
                onayliTeklif = teklif;
                mesajText.text = teklif + "$ mı? Tamam, anlaştık! Satabilirsin.";
            }
            else
            {
                mevcutMusteri.sabir -= (farkYuzdesi * 10f);
                mesajText.text = "Çok pahalı! Biraz daha inmelisin.";
                onayliTeklif = 0;
            }
        }
        else
        {
            if (farkYuzdesi >= 0.75f)
            {
                onayliTeklif = teklif;
                mesajText.text = teklif + "$ mı? Güzel fiyat, alabilirsin.";
            }
            else
            {
                float sabirKaybi = (1f / (farkYuzdesi + 0.05f)) * 5f;
                mevcutMusteri.sabir -= sabirKaybi;
                mesajText.text = "Bu fiyata hayatta olmaz!";
                onayliTeklif = 0;
            }
        }
        UpdateUI();
    }

    public void BuyItem()
    {
        if (onayliTeklif > 0 && playerMoney >= onayliTeklif)
        {
            playerMoney -= onayliTeklif;
            hasItem = true;
            SiradakiMusteriyiCagir();
        }
    }

    public void SellItem()
    {
        if (onayliTeklif > 0 && hasItem)
        {
            playerMoney += onayliTeklif;
            hasItem = false;
            GenerateNewMarketItem();
            SiradakiMusteriyiCagir();
        }
    }

    void UpdateUI()
    {
        if (moneyText != null) moneyText.text = "Para: " + playerMoney + "$";
        if (itemInfoText != null) itemInfoText.text = "Ürün: " + currentItemName;
        if (priceText != null) priceText.text = "Değer: " + esyaninGercekDegeri + "$";

        if (mevcutMusteri != null)
        {
            string rol = mevcutMusteri.alici ? "Alıcı" : "Satıcı";
            customerText.text = $"Müşteri: {mevcutMusteri.isim}\nDurum: {rol}\nSabır: %{(int)mevcutMusteri.sabir}";
            sabirSlider.value = mevcutMusteri.sabir / 100f;

            // BUTON KONTROLLERİ: Elimiz boşsa sadece satinAlButonu (Müşteri Satıcıyken) aktif olabilir.
            satinAlButonu.interactable = (onayliTeklif > 0 && !mevcutMusteri.alici && !hasItem);
            satButonu.interactable = (onayliTeklif > 0 && mevcutMusteri.alici && hasItem);
        }
    }
}