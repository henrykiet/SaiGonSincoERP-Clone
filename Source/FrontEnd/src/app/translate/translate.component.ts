import { Component } from '@angular/core'
import { TranslateService, TranslateModule } from '@ngx-translate/core'

@Component({
    selector: 'app-home',
    standalone: true,
    templateUrl: './translate.component.html',
    imports: [
        TranslateModule,
    ],
})
export class TranslateComponent {
    constructor(public translate: TranslateService) {
        this.translate.setDefaultLang(localStorage.getItem("language") ?? "vi")
    }

    changeLang(lang: string) {
        this.translate.use(lang)
    }
}
