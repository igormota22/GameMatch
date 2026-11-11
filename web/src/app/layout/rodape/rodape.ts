import { Component } from '@angular/core';
import { URL_FREETOGAME } from '../../core/configuracao/atribuicao';

@Component({
  selector: 'app-rodape',
  templateUrl: './rodape.html',
})
export class Rodape {
  protected readonly urlFreetogame = URL_FREETOGAME;
}
