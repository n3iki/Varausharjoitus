# Varausharjoitus // Joonas Nissinen LLPRIT21S // Joonas Nissinen LLPRIT21S
Tämä repositorio on LAB-ammattikorkeakoulun "Back-end -työkalut" -kurssille kuuluvan harjoitustyön repositorio.

# Varausharjoitus API
API:n tarkoitus on toimia backendinä esineiden varauspalvelulle. Varauspalvelussa luodaan käyttäjä, joka voi laittaa esineitään vuokralle ja vuokrata niitä muilta käyttäjiltä.

Kutsuihin tarvittava API-avain on "SATUNNAINENSTRINGI".

Controllerit toimivat ilman kontekstia joten kolmikerrosrakenteen pitäisi olla OK.

# Funktiot
API-funktioiden dokumentaatio löytyy Swaggerista ja mahdollisesti .pdf -tiedostona mikäli sen muistan lisätä.

# KNOWN ISSUES:
-Usealla käyttäjällä voi olla sama käyttäjätunnus (helppo korjata, mutta aika ei riitä)

-Pyyntöjen palautuskoodeila (200, 401, 204...) ei ole selitteitä dokumentoinnissa

-Kuvien käsittely API:ssa esim. ImageControllerilla on jätetty kokonaan pois. Kuvat ovat pelkkä linkki varauksen yhteydessä joten periaatteessa Image -luokkaa ei tarvitsisi ollenkaan, aika kuitenkin loppui kesken joten nyt vähän keskeneräisesti toteutettu mutta toimii.