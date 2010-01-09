/*
 * Copyright 2006 and onwards Sean Owen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.planetj.taste.impl.model.netflix;

import com.planetj.taste.model.Item;

import org.jetbrains.annotations.NotNull;

/**
 * @author Sean Owen
 * @since 1.3.5
 */
final class NetflixMovie implements Item {

	private final Integer id;
	private final String title;

	NetflixMovie(final Integer id, final String title) {
		if (id == null || title == null) {
			throw new IllegalArgumentException("ID or title is null");
		}
		this.id = id;
		this.title = title;
	}

	@NotNull
	public Object getID() {
		return id;
	}

	@NotNull
	String getTitle() {
		return title;
	}

	public boolean isRecommendable() {
		return true;
	}

	@Override
	public int hashCode() {
		return id.hashCode();
	}

	@Override
	public boolean equals(final Object obj) {
		return obj instanceof NetflixMovie && ((NetflixMovie) obj).id.equals(id);
	}

	public int compareTo(final Item item) {
		return this.id.compareTo(((NetflixMovie) item).id);
	}

	@NotNull
	@Override
	public String toString() {
		return id + ":" + title;
	}

}
