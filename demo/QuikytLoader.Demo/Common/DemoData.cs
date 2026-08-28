using QuikytLoader.Domain.Common;

namespace QuikytLoader.Demo.Common;

public static class DemoData
{
    public static readonly string[] DemoTitles =
    [
        "Short video",
        "A normal length video title for testing UI",
        "This is a very long video title that should test wrapping and overflow behavior in the UI",
        "🎵 Music video with emoji",
        "Video with special characters: тест видео 日本語"
    ];

    public static string RandomTitle => DemoTitles[Random.Shared.Next(DemoTitles.Length)];

    public static readonly string?[] DemoChannels =
    [
        "Channel",
        "VeryLongChannelNameThatMayBreakTheLayout",
        "Super Puper Very Long Channel Name That May Break The Layout But I Hope It Won't",
        "Short",
        "Demo Creator",
        null
    ];

    public static string? RandomChannel => DemoChannels[Random.Shared.Next(DemoChannels.Length)];

    public static readonly string[] DemoDescriptions =
    [
        "Death is not the opposite of life. Death is the opposite of birth. Life has no opposite. Eckhart Tolle describes his own experience of the world as light, insubstantial and dreamlike. Not because nothing matters, but because he has stopped confusing what is relatively important with what is absolutely important. The world loses its heaviness. Things still matter. They just do not have the crushing seriousness they once did. Your whole life, all the anxiety, all the suffering, all the drama — on a gravestone it is a dash. Two inches long.",

        "Смерть — не противоположность жизни. Смерть — противоположность рождению. У жизни нет противоположности. Экхарт Толле описывает своё восприятие мира как лёгкое, нематериальное и похожее на сон. Не потому, что ничто не имеет значения, а потому, что он перестал путать то, что относительно важно, с тем, что абсолютно важно. Мир теряет свою тяжесть. Вещи по-прежнему имеют значение. Просто в них больше нет той сокрушительной серьёзности, которая когда-то была. Вся ваша жизнь, вся тревога, все страдания, вся драма — на надгробии это всего лишь тире. Длиной в два дюйма.",

        "Der Tod ist nicht das Gegenteil des Lebens. Der Tod ist das Gegenteil der Geburt. Das Leben hat kein Gegenteil. Eckhart Tolle beschreibt seine eigene Erfahrung der Welt als leicht, substanzlos und traumähnlich. Nicht weil nichts von Bedeutung ist, sondern weil er aufgehört hat, das relativ Wichtige mit dem absolut Wichtigen zu verwechseln. Die Welt verliert ihre Schwere. Die Dinge sind weiterhin wichtig. Sie haben nur nicht mehr diese erdrückende Ernsthaftigkeit, die sie einst hatten. Dein ganzes Leben, all die Angst, all das Leid, all das Drama — auf einem Grabstein ist es ein Gedankenstrich. Zwei Zoll lang.",

        "Døden er ikke det modsatte af livet. Døden er det modsatte af fødslen. Livet har ingen modsætning. Eckhart Tolle beskriver sin egen oplevelse af verden som let, uhåndgribelig og drømmeagtig. Ikke fordi intet betyder noget, men fordi han er holdt op med at forveksle det, der er relativt vigtigt, med det, der er absolut vigtigt. Verden mister sin tyngde. Ting betyder stadig noget. De har bare ikke længere den knusende alvor, de engang havde. Hele dit liv, al angsten, al lidelsen, alt dramaet — på en gravsten er det en tankestreg. To tommer lang.",

        "La mort n’est pas le contraire de la vie. La mort est le contraire de la naissance. La vie n’a pas de contraire. Eckhart Tolle décrit sa propre expérience du monde comme légère, immatérielle et semblable à un rêve. Non pas parce que rien n’a d’importance, mais parce qu’il a cessé de confondre ce qui est relativement important avec ce qui est absolument important. Le monde perd sa lourdeur. Les choses ont toujours de l’importance. Elles n’ont simplement plus le caractère écrasant qu’elles avaient autrefois. Toute votre vie, toute l’anxiété, toute la souffrance, tout le drame — sur une pierre tombale, ce n’est qu’un tiret. Long de cinq centimètres.",

        "La morte non è l’opposto della vita. La morte è l’opposto della nascita. La vita non ha un opposto. Eckhart Tolle descrive la propria esperienza del mondo come leggera, inconsistente e simile a un sogno. Non perché nulla abbia importanza, ma perché ha smesso di confondere ciò che è relativamente importante con ciò che è assolutamente importante. Il mondo perde la sua pesantezza. Le cose continuano ad avere importanza. Semplicemente, non hanno più quella gravità schiacciante che avevano un tempo. Tutta la tua vita, tutta l’ansia, tutta la sofferenza, tutto il dramma — su una lapide è un trattino. Lungo cinque centimetri.",

        "死は生の反対ではない。死は誕生の反対だ。生には反対がない。エックハルト・トールは、自分が体験する世界を、軽やかで、実体がなく、夢のようなものだと表現しています。それは何もかもが重要ではないという意味ではありません。相対的に重要なものと、絶対的に重要なものを混同することをやめたからです。世界から重さが失われます。物事は依然として大切です。ただ、かつてのような押しつぶされるほどの深刻さはありません。あなたの人生すべて、不安も、苦しみも、ドラマも――墓石の上では、一本のダッシュです。長さはわずか5センチほどです。",

        "죽음은 삶의 반대가 아닙니다. 죽음은 탄생의 반대입니다. 삶에는 반대가 없습니다. 에크하르트 톨레는 자신이 경험하는 세상을 가볍고, 실체가 없으며, 꿈과 같다고 표현합니다. 아무것도 중요하지 않아서가 아니라, 상대적으로 중요한 것과 절대적으로 중요한 것을 더 이상 혼동하지 않기 때문입니다. 세상은 그 무게를 잃습니다. 사물은 여전히 중요합니다. 다만 예전처럼 짓누를 만큼 심각하지는 않습니다. 당신의 인생 전체, 모든 불안과 모든 고통, 모든 드라마는 묘비 위에서 하나의 대시일 뿐입니다. 길이는 5센티미터 정도입니다.",

        "De dood is niet het tegenovergestelde van het leven. De dood is het tegenovergestelde van geboorte. Het leven heeft geen tegenovergestelde. Eckhart Tolle beschrijft zijn eigen ervaring van de wereld als licht, onstoffelijk en droomachtig. Niet omdat niets ertoe doet, maar omdat hij niet langer verwart wat relatief belangrijk is met wat absoluut belangrijk is. De wereld verliest zijn zwaarte. Dingen doen er nog steeds toe. Ze hebben alleen niet meer de verpletterende ernst die ze ooit hadden. Je hele leven, alle angst, al het lijden, al het drama — op een grafsteen is het een streepje. Twee centimeter lang.",

        "Døden er ikke det motsatte av livet. Døden er det motsatte av fødselen. Livet har ingen motsetning. Eckhart Tolle beskriver sin egen opplevelse av verden som lett, uhåndgripelig og drømmeaktig. Ikke fordi ingenting betyr noe, men fordi han har sluttet å forveksle det som er relativt viktig med det som er absolutt viktig. Verden mister sin tyngde. Ting betyr fortsatt noe. De har bare ikke lenger den knusende alvoret de en gang hadde. Hele livet ditt, all angsten, all lidelsen, alt dramaet — på en gravstein er det en tankestrek. To tommer lang.",

        "A morte não é o oposto da vida. A morte é o oposto do nascimento. A vida não tem oposto. Eckhart Tolle descreve sua própria experiência do mundo como leve, insubstancial e semelhante a um sonho. Não porque nada importe, mas porque ele deixou de confundir aquilo que é relativamente importante com aquilo que é absolutamente importante. O mundo perde seu peso. As coisas continuam tendo importância. Elas simplesmente já não carregam a seriedade esmagadora que um dia tiveram. Toda a sua vida, toda a ansiedade, todo o sofrimento, todo o drama — em uma lápide, isso é apenas um travessão. Com cinco centímetros de comprimento.",

        "La muerte no es lo contrario de la vida. La muerte es lo contrario del nacimiento. La vida no tiene contrario. Eckhart Tolle describe su propia experiencia del mundo como algo ligero, inmaterial y onírico. No porque nada importe, sino porque ha dejado de confundir lo que es relativamente importante con lo que es absolutamente importante. El mundo pierde su peso. Las cosas siguen importando. Simplemente ya no tienen la gravedad aplastante que alguna vez tuvieron. Toda tu vida, toda la ansiedad, todo el sufrimiento, todo el drama — en una lápida es un guion. De cinco centímetros de largo.",

        "Döden är inte motsatsen till livet. Döden är motsatsen till födelsen. Livet har ingen motsats. Eckhart Tolle beskriver sin egen upplevelse av världen som lätt, ogripbar och drömlik. Inte för att ingenting spelar någon roll, utan för att han har slutat förväxla det som är relativt viktigt med det som är absolut viktigt. Världen förlorar sin tyngd. Saker och ting är fortfarande viktiga. De har bara inte längre den krossande allvarlighet som de en gång hade. Hela ditt liv, all ångest, allt lidande, allt drama — på en gravsten är det ett streck. Två tum långt.",

        "死亡不是生命的反面。死亡是出生的反面。生命没有反面。埃克哈特·托利将他对这个世界的体验描述为轻盈、无实体、如梦一般。并不是因为没有什么重要，而是因为他已经不再把相对重要的事物与绝对重要的事物混为一谈。世界失去了它的沉重感。事物依然重要。只是它们不再具有曾经那种令人窒息的严肃性。你的一生，所有的焦虑、所有的痛苦、所有的戏剧——在墓碑上不过是一道横线。只有两英寸长。"
    ];

    public static string RandomDescription => DemoDescriptions[Random.Shared.Next(DemoDescriptions.Length)];

    public static readonly TimeSpan[] DemoDurations =
    [
        TimeSpan.FromSeconds(15),
        TimeSpan.FromMinutes(3),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.Zero
    ];

    public static TimeSpan RandomDuration => DemoDurations[Random.Shared.Next(DemoDurations.Length)];

    public static readonly Dictionary<string, string[]> SubtitleSamples = new()
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
}
