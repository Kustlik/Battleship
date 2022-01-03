# Battleship
- Przemyślenie technologii w której zacznę pracę, wybrałem WPF, ponieważ jest to frontend z którym miałem już styczność.
- Przemyślenie całego obrazu projektu, jak będzie wyglądać gra w statki
- Pierwsza plansza będzie zawarta po lewej stronie aplikacji, druga plansza na prawo.
- Jak długo będą trwały tury graczy? Jako że jest to symulacja, uznałem że 5 sekund czasu będzie wystarczająco by zorientować się, co dzieje się na planszy. Jednak niektórzy mogą uznać, że jest to za mało czasu, dlatego ustawię tabelkę do wpisania własnej wartości czasu, by każdy mógł dopasować prędkość do siebie.
- Przemyślenie w jaki sposób przechować pola, by był do nich łatwy dostęp. Macierz będzie dobrą opcją do przechowywania takich inforamcji. Metoda która będzie tworzyć macierz jest napisana także w taki sposób, by obsługiwać rózne rozmiary. Oryginalny rozmiar to 10x10, natomiast uwzględnilem elastyczność w przypadku przyszlych modyfikacji. Macierz bedzie przechowywac specjalne enumeracje, odpowiadające za aktualny stan komórki.
- Wprowadzenie Enumeracji, ułatwiających rozeznanie się w pętlach oraz dodanie typów statków.
- Dodałem także Enumerację kierunkowe, na których oprę się w trakcie sprawdzania pól. Myślałem nad zastosowaniem rekursji do skanowania pól, natomiast uznałem, że może to skomplikować kod za bardzo w przypadku implementacji inteligentnego celowania.
- W jaki sposób będą rozstawiane statki na platformie? Po przeszukaniu internetu, dowiedziałem się, że w oryginalnej wersji, statki mogą być rozmieszczane w sąsiedztwie od siebie. Wiec tworzę program w oparciu o taką wersję.
- Stworzenie klasy statków, każdy statek musi zawierać informacje o swoich parametrach, konstruktor dba o przypisanie wartości długości statku według jego rodzaju. length zostawiony prywatny, ponieważ nie jest to wartość do modyfikacji. Koordynaty dodane by ulatwić lokalizacje na planszy. Zniszczone fragmenty, by móc potem je przyrównać z koordynatami i zdecydować, kiedy statek zatonie.
- W jaki sposób napisać metode stawiającą statki, by nie wychodziły za planszę, ani nie nachodziły na siebie? Najpierw program wybiera losowe pole na planszy dla wczesniej wybranego statku, który zawiera informacje o długosci (Szerokość nie jest potrzebna, gdyż każdy statek ma szerokość równą jeden). gdy wybierze pole, sprawdza sąsiednie kratki, czy dla danego typu statku znajdzie sie miejsce. Jeżeli w danym kierunku można wybudowac statek, dodaje go do tablicy, z której potem będzie losowany kierunek (by wylosować także kierunek stawiania statków). Kiedy miejsce startu i kierunek budowy zostają wybrane, następuje postawienie statku na planszy, jednoczesnie zapisujac koordynaty punktów statku (jest to potrzebne, by sprawdzic potem, czy statek zostal zatopiony, tym bardziej że statki moga ze soba sasiadowac, co moze byc problematyczne przy rozróżnieniu punktów między statkami).
- Program musi oczywiscie też rozróżnic krańce planszy, dlatego podczas sprawdzania wolnych oczek podczas wybierania kierunku, najpierw sprawdzane jest, czy statek zmiesci sie do granicy mapy (mapa zaczyna się od punktu x = 1 i y = 1, przesunąłem te wartosci z 0 by nie powodować zamieszania w kodzie).
- Gdzie powinienem zamieścić informacje o trafionych polach, zatopionych statkach itp? Nie wprowadzam żadnego systemu punktacji, bo uważam że w tej grze nie ma to sensu. Jako iż jest to pojedynek 2 komputerów, nie trzeba zbytnio zatajać informacji, więc komputer może wyciągać informacje o polu na bieżąco. Gdyby to byl pojedynek graczy, stworzyłbym dodatkową macierz dla tego gracza, która przechowywałaby informację o trafionych polach i wizualizowała na bieżąco oraz dodałbym element klasy Player do przechowywania nowych danych o przeciwniku.
- Dodałem także pare metod skanujących planszę, ułatwiającą znaleźć punkty odniesienia dla graczy.
- Umieszczam metody obsługujące planszę bezpośrednio w klasie planszy. Jest to potrzebne, ponieważ każdy gracz musi obsługiwać swoją własną planszę i ułatwia to bezpośrednią komunikację z planszą. Pozostawiłem board matrix nie publiczny, by dostęp do modyfikacji pól został ograniczony. Dodałem metody obsługujące tą zmienną.
- Utworzenie klasy która będzię obsługiwać aktualną rozgrywkę, wywołanie gry będzie przypisywać wartości opóźnienia tury oraz wylosowanie pierwszego gracza.
- Stworzenie metody która będzie obsługiwać turę dla obu graczy, na początku następuje losowanie gracza. Tura musi byc obsługiwana z opóźnieniem, dlatego nie obejdzie sie bez oczekiwania Task.delay, by program był wciąż responsywny. Dodałem też metode obsługującą pauzę dla wszystkich tasków oraz token anulujący. Będzie to potrzebne w przypadku restartu gry.
- Stworzenie prostego systemu celowania, polegającego na losowym wybieraniu komórek co turę gracza.
- Następnym krokiem jest utworzenie systemu strzelania i tury. Oczywiscie komputer będzie celował losowo, jednak muszę zamontować mu jakiś system, który pozwoli na inteligentne celowanie. Jeżeli juz trafi w statek, będzie sprawdzał sąsiadujące kratki, aż nie zatopi okrętu. Musi też mieć na uwadze kratki, które zostały spudłowane. Trafienie okrętu spowoduje ustawienie punktu odniesienia dla gracza, od którego bedzie sprawdzał wszystkie poboczne kratki. Jeżeli trafi drugi punkt, moze wywnioskować oś, po której sie poruszać. Jeżeli gracz trafi na slepy zaułek, a statek dalej nie został zatopiony, gracz powróci do pierwszego punktu i zacznie sprawdzać przeciwną stronę. W przypadku, gdy gracz po drodze napotkał jakiś inny statek (jako że mogą ze sobą sąsiadować, może sie tak zdarzyć), wtedy gracz obiera znaleziony punkt jako głowny punkt odniesienia i zaczyna badać kratki na około, zaraz po zatopieniu poprzedniego statku. Jeżeli gracz natknie się na ślepe zaułki z obu stron, a statek dalej jest niezatopiony, wtedy zmienia kierunek badania i idzie wzdłuż niego. Gracz musi też brać pod uwage już zatopione statki, musi więc omijać kratki niesprawdzone, do których najmniejszy statek ostały się na planszy, nie zmieści się. Jak w przypadku budowy, nalezy też uważac na brzegi planszy, traktuję je jako juz "trafione przez gracza".
- Pomyślałem, że dodanie baru progresywnego, będzie łatwiej wizualizować turę.
- Dodanie przycisku resetującego i zaimplementowanie resetowania wszystkich danych z poprzedniej rozgrywki, miedzy innymi wymazanie statków, obecnego punktu celowania i wyczyszczenie planszy dla obu graczy. Przerwanie tury tokenem anulującym. Zablokowalem takze interfejs w trakcie działania gry.
- Dodanie panelu, który pojawi się po zakończeniu rozgrywki.
