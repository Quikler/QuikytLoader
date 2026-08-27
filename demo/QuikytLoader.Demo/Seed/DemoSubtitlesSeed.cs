using QuikytLoader.Domain.Common;

namespace QuikytLoader.Demo.Seed;

internal sealed class DemoSubtitlesSeed
{
    private readonly Dictionary<string, string[]> SubtitleSamples = new()
    {
        [Language.English.Iso6391Code] =
        [
            "Welcome back everyone. Today we're looking at a simple example.",
            "The results may vary depending on your configuration.",
            "Thanks for watching. Don't forget to subscribe."
        ],

        [Language.Russian.Iso6391Code] =
        [
            "Всем привет. Сегодня мы рассмотрим простой пример.",
            "Результаты могут отличаться в зависимости от настроек.",
            "Спасибо за просмотр. До встречи в следующем видео."
        ],

        [Language.German.Iso6391Code] =
        [
            "Willkommen zurück. Heute schauen wir uns ein einfaches Beispiel an.",
            "Die Ergebnisse können je nach Konfiguration variieren.",
            "Vielen Dank fürs Zuschauen."
        ],

        [Language.Danish.Iso6391Code] =
        [
            "Velkommen tilbage. I dag ser vi på et enkelt eksempel.",
            "Resultatet kan variere afhængigt af din opsætning.",
            "Tak fordi du så med."
        ],

        [Language.French.Iso6391Code] =
        [
            "Bienvenue à tous. Aujourd'hui nous allons voir un exemple simple.",
            "Les résultats peuvent varier selon votre configuration.",
            "Merci d'avoir regardé cette vidéo."
        ],

        [Language.Italian.Iso6391Code] =
        [
            "Benvenuti. Oggi vedremo un semplice esempio.",
            "I risultati possono variare in base alla configurazione.",
            "Grazie per aver guardato il video."
        ],

        [Language.Japanese.Iso6391Code] =
        [
            "皆さん、こんにちは。今日は簡単な例を紹介します。",
            "設定によって結果が異なる場合があります。",
            "ご視聴ありがとうございました。"
        ],

        [Language.Korean.Iso6391Code] =
        [
            "안녕하세요. 오늘은 간단한 예제를 살펴보겠습니다.",
            "설정에 따라 결과가 달라질 수 있습니다.",
            "시청해 주셔서 감사합니다."
        ],

        [Language.Dutch.Iso6391Code] =
        [
            "Welkom terug. Vandaag bekijken we een eenvoudig voorbeeld.",
            "De resultaten kunnen verschillen afhankelijk van de instellingen.",
            "Bedankt voor het kijken."
        ],

        [Language.Norwegian.Iso6391Code] =
        [
            "Velkommen tilbake. I dag ser vi på et enkelt eksempel.",
            "Resultatet kan variere avhengig av oppsettet.",
            "Takk for at du så på."
        ],

        [Language.Portuguese.Iso6391Code] =
        [
            "Bem-vindos. Hoje veremos um exemplo simples.",
            "Os resultados podem variar dependendo da configuração.",
            "Obrigado por assistir."
        ],

        [Language.Spanish.Iso6391Code] =
        [
            "Bienvenidos. Hoy veremos un ejemplo sencillo.",
            "Los resultados pueden variar según la configuración.",
            "Gracias por ver el vídeo."
        ],

        [Language.Swedish.Iso6391Code] =
        [
            "Välkommen tillbaka. Idag tittar vi på ett enkelt exempel.",
            "Resultatet kan variera beroende på dina inställningar.",
            "Tack för att du tittade."
        ],

        [Language.Chinese.Iso6391Code] =
        [
            "大家好，今天我们来看一个简单的示例。",
            "结果可能会因配置不同而有所变化。",
            "感谢观看，我们下次再见。"
        ]
    };

    public IReadOnlyDictionary<string, string> Generate()
    {
        var result = new Dictionary<string, string>();

        foreach (var (language, sampleSubtitles) in SubtitleSamples)
        {
            // 60% chance subtitles exist for this language
            if (Random.Shared.NextDouble() < 0.6)
            {
                result[language] = string.Join(
                    Random.Shared.Next(0, 2) == 0 ? ' ' : '\n',
                    Enumerable.Range(0, Random.Shared.Next(4, 10))
                        .Select(_ => sampleSubtitles[Random.Shared.Next(sampleSubtitles.Length)]));
            }
        }

        return result;
    }
}
