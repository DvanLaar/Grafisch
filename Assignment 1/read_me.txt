Graphics Assignment 1
door
Daan van Laar   - 5518741
Ludo Pulles     - 5727146
Jasper Everink  - 5649137

Bonus Exercise 5:
Besturing:
Pijltjestoetsen - Bewegen
Z               - Uitzoomen
X               - Inzoomen

Uitleg:
De applicatie plot twee functies. Een functie is van de vorm y=f(x) (rood).
Deze wordt op een groot aantal waarden van x geevalueerd, waartussen lijnstukken getekent worden.
De andere functie is van de vorm r=f(theta) (blauw) in poolcoordinaten (x=r*cos(theta) en y=r*sin(theta))
over een bereik van 0 tot en met 10 Pi voor theta. Deze functie wordt ook op een groot aantal waarden van theta
geevalueerd, waartussen lijnstukken worden getekend.
Het rooster op de achtergrond bestaat uit een licht groene lijn voor de x- en y-as, een groene lijn voor elk interval van 1
en hiertussen nog een donker groene lijn voor de intervallen van 0,5.


Bonus Exercise 10:
Besturing:
W               - Verhoog het lichtpunt
S               - Verlaag het lichtpunt
A               - Verplaats het lichtpunt positief over de X-as
D               - Verplaats het lichtpunt negatief over de X-as
Q               - Verplaats het lichtpunt positief over de Y-as
E               - Verplaats het lichtpunt negatief over de Y-as
Pijl omhoog     - Draai camera omhoog
Pijl omlaag     - Draai camera omlaag

Uitleg:
Voor de diffuse shading moeten eerst de normalen worden bepaalt.
Voor elk vakje (1 pixel van de heightmap) worden er twee triangles aangemaakt.
Voor elk van deze triangles wordt de normaal bepaald als genormaalizeerd cross-product van
twee vectoren die de triangle opspannen. De normaal per vertex wordt dan bepaald als gemiddelde
van de normalen van de aanliggende triangles. (Als de vertex aan de rand ligt wordt er extra
gemiddeld met z-as unit vectors om het bepalen van de normaalen makkelijker te maken)
Deze normalen worden vervolgens aan een attribute verbonden om ze te kunnen gebruiken binnen de shaders.
Doormiddel van uniform variables wordt een locatie van een lichtpunt en de kleur/intensiteit
van een lichtpunt naar de shader gestuurd.
De normalen worden door de vertex shader naar de fragment shader gestuurd (samen met de positie).
In de fragment shader wordt eerst de helderheid van het licht op de pixel bepaald (op basis van het inverse
kwadraat van de afstand tot het lichtpunt) en het inproduct van de normaal met een vector
richting de lichtbron. Voor de uiteindelijke kleur wort deze helderheid, het inproduct, de lichtkleur
en de kleur van de vertex zelf met elkaar vermenigvuldigt.
(Er zit geen Ambient belichting bij)

Verder hebben we ook nog een punt toegevoegd op de positie van de lightbron. Deze verplaatst mee als men de lichtbron beweegt. Zo zijn goed de belichtseffecten in te zien in het programma.
Dit punt werd ook door de shaders aangeroepen dus hebben we een speciaal geval in de shader geschreven voor het geval dat een punt zich signficant dicht bij de lichtbron in zit. In dit geval neemt het de kleur van het licht aan.
